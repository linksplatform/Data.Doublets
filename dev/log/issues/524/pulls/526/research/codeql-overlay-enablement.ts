// Excerpt of src/config-utils.ts at pinned 9b3254085494b55a0f7c4c2b3d2d52cc712ce370
  }
}

/**
 * Maps languages to their overlay analysis feature flags. Only languages that
 * are GA or in staff-ship for overlay analysis are included here. Languages
 * without an entry will have overlay analysis disabled.
 */
const OVERLAY_ANALYSIS_FEATURES: Partial<Record<Language, Feature>> = {
  cpp: Feature.OverlayAnalysisCpp,
  csharp: Feature.OverlayAnalysisCsharp,
  go: Feature.OverlayAnalysisGo,
  java: Feature.OverlayAnalysisJava,
  javascript: Feature.OverlayAnalysisJavascript,
  python: Feature.OverlayAnalysisPython,
  ruby: Feature.OverlayAnalysisRuby,
};

const OVERLAY_ANALYSIS_CODE_SCANNING_FEATURES: Partial<
  Record<Language, Feature>
> = {
  cpp: Feature.OverlayAnalysisCodeScanningCpp,
  csharp: Feature.OverlayAnalysisCodeScanningCsharp,
  go: Feature.OverlayAnalysisCodeScanningGo,
  java: Feature.OverlayAnalysisCodeScanningJava,
  javascript: Feature.OverlayAnalysisCodeScanningJavascript,
  python: Feature.OverlayAnalysisCodeScanningPython,
  ruby: Feature.OverlayAnalysisCodeScanningRuby,
};

/**
 * Checks whether the overlay analysis feature is enabled for the given
 * languages and configuration.
 */
async function checkOverlayAnalysisFeatureEnabled(
  features: FeatureEnablement,
  codeql: CodeQL,
  languages: Language[],
  codeScanningConfig: UserConfig,
): Promise<Result<void, OverlayDisabledReason>> {
  if (!(await features.getValue(Feature.OverlayAnalysis, codeql))) {
    return new Failure(OverlayDisabledReason.OverallFeatureNotEnabled);
  }
  let enableForCodeScanningOnly = false;
  for (const language of languages) {
    const feature = OVERLAY_ANALYSIS_FEATURES[language];
    if (feature && (await features.getValue(feature, codeql))) {
      continue;
    }
    const codeScanningFeature =
      OVERLAY_ANALYSIS_CODE_SCANNING_FEATURES[language];
    if (
      codeScanningFeature &&
      (await features.getValue(codeScanningFeature, codeql))
    ) {
      enableForCodeScanningOnly = true;
      continue;
    }
    return new Failure(OverlayDisabledReason.LanguageNotEnabled);
  }
  if (enableForCodeScanningOnly) {
    // A code-scanning configuration runs only the (default) code-scanning suite
    // if the default queries are not disabled, and no packs, queries, or
    // query-filters are specified.
    const usesDefaultQueriesOnly =
      codeScanningConfig["disable-default-queries"] !== true &&
      codeScanningConfig.packs === undefined &&
      codeScanningConfig.queries === undefined &&
      codeScanningConfig["query-filters"] === undefined;
    if (!usesDefaultQueriesOnly) {
      return new Failure(OverlayDisabledReason.NonDefaultQueries);
    }
  }
  return new Success(undefined);
}

/**
 * Returns the minimum available disk space (in MB) required to perform overlay
 * analysis, which is the lowest threshold enabled by a feature flag, or the
 * default threshold if no such feature flag is enabled.
 */
async function getMinimumDiskSpaceMb(
  features: FeatureEnablement,
): Promise<number> {
  let minimumMb = OVERLAY_MINIMUM_AVAILABLE_DISK_SPACE_MB;
  for (const [feature, thresholdMb] of Object.entries(
    OVERLAY_MINIMUM_DISK_SPACE_MB_BY_FEATURE,
  )) {
    if (await features.getValue(feature as FeatureWithoutCLI)) {
      minimumMb = Math.min(minimumMb, thresholdMb);
    }
  }
  return minimumMb;
}

/** Checks if the runner has enough disk space for overlay analysis. */
function runnerHasSufficientDiskSpace(
  diskUsage: DiskUsage,
  logger: Logger,
  minimumDiskSpaceMb: number,
): boolean {
  const diskSpaceMb = Math.round(diskUsage.numAvailableBytes / 1_000_000);
  if (diskUsage.numAvailableBytes < minimumDiskSpaceMb * 1_000_000) {
    logger.info(
      `Setting overlay database mode to ${OverlayDatabaseMode.None} ` +
        `due to insufficient disk space (${diskSpaceMb} MB, needed ${minimumDiskSpaceMb} MB).`,
    );
    return false;
  }

  logger.debug(
    `Disk space available for CodeQL analysis is ${diskSpaceMb} MB, which is at or above the ` +
      `minimum of ${minimumDiskSpaceMb} MB.`,
  );
  return true;
}

/** Checks if the runner has enough memory for overlay analysis. */
async function runnerHasSufficientMemory(
  codeql: CodeQL,
  ramInput: string | undefined,
  logger: Logger,
): Promise<boolean> {
  if (
    await codeQlVersionAtLeast(
      codeql,
      CODEQL_VERSION_REDUCED_OVERLAY_MEMORY_USAGE,
    )
  ) {
    logger.debug(
      `Skipping memory check for overlay analysis because CodeQL version is at least ${CODEQL_VERSION_REDUCED_OVERLAY_MEMORY_USAGE}.`,
    );
    return true;
  }

  const memoryFlagValue = getCodeQLMemoryLimit(ramInput, logger);
  if (memoryFlagValue < OVERLAY_MINIMUM_MEMORY_MB) {
    logger.info(
      `Setting overlay database mode to ${OverlayDatabaseMode.None} ` +
        `due to insufficient memory for CodeQL analysis (${memoryFlagValue} MB, needed ${OVERLAY_MINIMUM_MEMORY_MB} MB).`,
    );
    return false;
  }

  logger.debug(
    `Memory available for CodeQL analysis is ${memoryFlagValue} MB, which is at or above the minimum of ${OVERLAY_MINIMUM_MEMORY_MB} MB.`,
  );
  return true;
}

/**
 * Checks if the runner has sufficient disk space and memory for overlay
 * analysis.
 */
async function checkRunnerResources(
  codeql: CodeQL,
  features: FeatureEnablement,
  diskUsage: DiskUsage,
  ramInput: string | undefined,
  logger: Logger,
): Promise<Result<void, OverlayDisabledReason>> {
  const minimumDiskSpaceMb = await getMinimumDiskSpaceMb(features);
  if (!runnerHasSufficientDiskSpace(diskUsage, logger, minimumDiskSpaceMb)) {
    return new Failure(OverlayDisabledReason.InsufficientDiskSpace);
  }
  if (!(await runnerHasSufficientMemory(codeql, ramInput, logger))) {
    return new Failure(OverlayDisabledReason.InsufficientMemory);
  }
  return new Success(undefined);
}

interface EnabledOverlayConfig {
  overlayDatabaseMode: Exclude<OverlayDatabaseMode, OverlayDatabaseMode.None>;
  useOverlayDatabaseCaching: boolean;
  overlayModeSetExplicitly: boolean;
}

/**
 * Calculate and validate the overlay database mode and caching to use.
 *
 * - If the environment variable `CODEQL_OVERLAY_DATABASE_MODE` is set, use it.
 *   In this case, the workflow is responsible for managing database storage and
 *   retrieval, and the action will not perform overlay database caching. Think
 *   of it as a "manual control" mode where the calling workflow is responsible
 *   for making sure that everything is set up correctly.
 * - Otherwise, if `Feature.OverlayAnalysis` is enabled, calculate the mode
 *   based on what we are analyzing. Think of it as a "automatic control" mode
 *   where the action will do the right thing by itself.
 *   - If we are analyzing a pull request, use `Overlay` with caching.
 *   - If we are analyzing the default branch, use `OverlayBase` with caching.
 * - Otherwise, use `None`.
 *
 * For `Overlay` and `OverlayBase`, the function performs further checks and
 * reverts to `None` if any check should fail.
 *
 * @returns A `Success` containing the overlay database mode and whether the
 * action should perform overlay-base database caching, or a `Failure`
 * containing the reason why overlay analysis is disabled.
 */
export async function checkOverlayEnablement(
  codeql: CodeQL,
  features: FeatureEnablement,
  languages: Language[],
  repositoryRoot: string | undefined,
  sourceRoot: string,
  buildMode: BuildMode | undefined,
  ramInput: string | undefined,
  codeScanningConfig: UserConfig,
  repositoryProperties: RepositoryProperties,
  gitVersion: GitVersionInfo | undefined,
  logger: Logger,
): Promise<Result<EnabledOverlayConfig, OverlayDisabledReason>> {
  const modeEnv = process.env.CODEQL_OVERLAY_DATABASE_MODE;
  // Any unrecognized CODEQL_OVERLAY_DATABASE_MODE value will be ignored and
  // treated as if the environment variable was not set.
  if (
    modeEnv === OverlayDatabaseMode.Overlay ||
    modeEnv === OverlayDatabaseMode.OverlayBase ||
    modeEnv === OverlayDatabaseMode.None
  ) {
    logger.info(
      `Setting overlay database mode to ${modeEnv} ` +
        "from the CODEQL_OVERLAY_DATABASE_MODE environment variable.",
    );
    if (modeEnv === OverlayDatabaseMode.None) {
      return new Failure(OverlayDisabledReason.DisabledByEnvironmentVariable);
    }
    return validateOverlayDatabaseMode(
      modeEnv,
      false,
      true,
      codeql,
      languages,
      repositoryRoot,
      sourceRoot,
      buildMode,
      gitVersion,
      logger,
    );
  }

  if (repositoryProperties[RepositoryPropertyName.DISABLE_OVERLAY] === true) {
    logger.info(
      `Setting overlay database mode to ${OverlayDatabaseMode.None} ` +
        `because the ${RepositoryPropertyName.DISABLE_OVERLAY} repository property is set to true.`,
    );
    return new Failure(OverlayDisabledReason.DisabledByRepositoryProperty);
  }

  const featureResult = await checkOverlayAnalysisFeatureEnabled(
    features,
    codeql,
    languages,
    codeScanningConfig,
  );
  if (featureResult.isFailure()) {
    return featureResult;
  }

  const performResourceChecks = !(await features.getValue(
    Feature.OverlayAnalysisSkipResourceChecks,
    codeql,
  ));
  const checkOverlayStatus = await features.getValue(
    Feature.OverlayAnalysisStatusCheck,
  );
  const needDiskUsage = performResourceChecks || checkOverlayStatus;
  const diskUsage = needDiskUsage ? await checkDiskUsage(logger) : undefined;
  if (needDiskUsage && diskUsage === undefined) {
    logger.warning(
      `Unable to determine disk usage, therefore setting overlay database mode to ${OverlayDatabaseMode.None}.`,
    );
    return new Failure(OverlayDisabledReason.UnableToDetermineDiskUsage);
  }
  const resourceResult =
    performResourceChecks && diskUsage !== undefined
      ? await checkRunnerResources(
          codeql,
          features,
          diskUsage,
          ramInput,
          logger,
        )
      : new Success<void>(undefined);
  if (resourceResult.isFailure()) {
    return resourceResult;
  }
  if (
    checkOverlayStatus &&
    diskUsage !== undefined &&
    (await shouldSkipOverlayAnalysis(codeql, languages, diskUsage, logger))
  ) {
    logger.info(
      `Setting overlay database mode to ${OverlayDatabaseMode.None} ` +
        "because overlay analysis previously failed with this combination of languages, " +
        "disk space, and CodeQL version.",
    );
    return new Failure(OverlayDisabledReason.SkippedDueToCachedStatus);
  }

  let overlayDatabaseMode: OverlayDatabaseMode;
  if (isAnalyzingPullRequest()) {
    overlayDatabaseMode = OverlayDatabaseMode.Overlay;
    logger.info(
      `Setting overlay database mode to ${overlayDatabaseMode} ` +
        "with caching because we are analyzing a pull request.",
    );
  } else if (await isAnalyzingDefaultBranch(getEnv(), repositoryRoot)) {
    overlayDatabaseMode = OverlayDatabaseMode.OverlayBase;
    logger.info(
      `Setting overlay database mode to ${overlayDatabaseMode} ` +
        "with caching because we are analyzing the default branch.",
    );
  } else {
    return new Failure(OverlayDisabledReason.NotPullRequestOrDefaultBranch);
  }

  return validateOverlayDatabaseMode(
    overlayDatabaseMode,
    true,
    false,
    codeql,
    languages,
    repositoryRoot,
    sourceRoot,
    buildMode,
    gitVersion,
    logger,
  );
}

/**
 * Validates that the given overlay database mode is compatible with the current
 * configuration (build mode, CodeQL version, git repository, git version). Returns
 * the mode unchanged if all checks pass, or falls back to `None` with the
 * appropriate disabled reason.
 */
async function validateOverlayDatabaseMode(
  overlayDatabaseMode: Exclude<OverlayDatabaseMode, OverlayDatabaseMode.None>,
  useOverlayDatabaseCaching: boolean,
  overlayModeSetExplicitly: boolean,
  codeql: CodeQL,
  languages: Language[],
  repositoryRoot: string | undefined,
  sourceRoot: string,
  buildMode: BuildMode | undefined,
  gitVersion: GitVersionInfo | undefined,
  logger: Logger,
): Promise<Result<EnabledOverlayConfig, OverlayDisabledReason>> {
  if (
    buildMode !== BuildMode.None &&
    (
      await Promise.all(
        languages.map(
          async (l) =>
            l !== BuiltInLanguage.go && // Workaround to allow overlay analysis for Go with any build
            // mode, since it does not yet support BMN. The Go autobuilder and/or extractor will
            // ensure that overlay-base databases are only created for supported Go build setups,
            // and that we'll fall back to full databases in other cases.
            (await codeql.isTracedLanguage(l)),
        ),
      )
    ).some(Boolean)
  ) {
    logger.warning(
      `Cannot build an ${overlayDatabaseMode} database because ` +
        `build-mode is set to "${buildMode}" instead of "none". ` +
        "Falling back to creating a normal full database instead.",
    );
    return new Failure(OverlayDisabledReason.IncompatibleBuildMode);
  }
  if (!(await codeQlVersionAtLeast(codeql, CODEQL_OVERLAY_MINIMUM_VERSION))) {
    logger.warning(
      `Cannot build an ${overlayDatabaseMode} database because ` +
        `the CodeQL CLI is older than ${CODEQL_OVERLAY_MINIMUM_VERSION}. ` +
        "Falling back to creating a normal full database instead.",
    );
    return new Failure(OverlayDisabledReason.IncompatibleCodeQl);
  }
  if (repositoryRoot === undefined) {
    logger.warning(
      `Cannot build an ${overlayDatabaseMode} database because ` +
        `the source root "${sourceRoot}" is not inside a git repository. ` +
        "Falling back to creating a normal full database instead.",
    );
    return new Failure(OverlayDisabledReason.NoGitRoot);
  }
  if (hasSubmodules(repositoryRoot)) {
    if (gitVersion === undefined) {
      logger.warning(
        `Cannot build an ${overlayDatabaseMode} database because ` +
          "the repository has submodules and the Git version could not be determined. " +
          "Falling back to creating a normal full database instead.",
      );
      return new Failure(OverlayDisabledReason.IncompatibleGit);
    }
    if (
      !gitVersion.isAtLeast(GIT_MINIMUM_VERSION_FOR_OVERLAY_WITH_SUBMODULES)
    ) {
      logger.warning(
        `Cannot build an ${overlayDatabaseMode} database because ` +
          "the repository has submodules and the installed Git version is older " +
          `than ${GIT_MINIMUM_VERSION_FOR_OVERLAY_WITH_SUBMODULES}. ` +
          "Falling back to creating a normal full database instead.",
      );
      return new Failure(OverlayDisabledReason.IncompatibleGit);
    }
  }

  return new Success({
    overlayDatabaseMode,
    useOverlayDatabaseCaching,
    overlayModeSetExplicitly,
  });
}

/**
 * Determines whether TRAP caching is enabled for the current run.
 *
 * @returns Whether TRAP caches should be downloaded and uploaded.
 */
export function isTrapCachingEnabled(
  overlayDatabaseMode: OverlayDatabaseMode,
): boolean {
  // If the workflow specified something, always respect that.
  const trapCaching = getOptionalInput("trap-caching");
  if (trapCaching !== undefined) return trapCaching === "true";

  // On self-hosted runners which may have slow network access, disable TRAP caching by default.
  if (!looksLikeHostedRunner()) return false;

  // Overlay analysis supersedes TRAP caching, so disable TRAP caching by default when it is active.
  if (overlayDatabaseMode !== OverlayDatabaseMode.None) {
    return false;
  }

  // Otherwise, enable TRAP caching.
  return true;
}

async function setCppTrapCachingEnvironmentVariables(
  config: Config,
  logger: Logger,
): Promise<void> {
  if (config.languages.includes(BuiltInLanguage.cpp)) {
    const envVar = "CODEQL_EXTRACTOR_CPP_TRAP_CACHING";
    if (process.env[envVar]) {
      logger.info(
        `Environment variable ${envVar} already set, leaving it unchanged.`,
      );
    } else if (config.trapCaches[BuiltInLanguage.cpp]) {
      logger.info("Enabling TRAP caching for C/C++.");
      exportEnvVar(envVar, "true");
    } else {
      logger.debug(`Disabling TRAP caching for C/C++.`);
      exportEnvVar(envVar, "false");
    }
  }
}

function dbLocationOrDefault(
  dbLocation: string | undefined,
  tempDir: string,
): string {
  return dbLocation || path.resolve(tempDir, "codeql_databases");
}

/**
 * Gets the path for the CodeQL Action-generated configuration file,
 * which is used to store the `config` input.
