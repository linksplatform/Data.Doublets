FROM kjanat/actionlint:1.17.0@sha256:d484cccadf56b6a356466ade9af4c8e05c5dce1badaf0a9ecd0d7639d68d9c92 AS binaries
FROM python:3.15.2-alpine@sha256:d499023466546cd88a358f80b46fba4016f32597bc4482bb1c47b483a07d457d
COPY --from=binaries /usr/local/bin/actionlint /usr/local/bin/shellcheck /usr/local/bin/
COPY scripts/requirements.lock.txt /tmp/requirements.lock.txt
RUN pip install --no-cache-dir --upgrade --constraint /tmp/requirements.lock.txt pip pyflakes
WORKDIR /work
ENTRYPOINT ["actionlint"]
