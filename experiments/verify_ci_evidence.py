"""Cross-check a downloaded issue-524 CI run against its raw reports and logs.

Usage: python experiments/verify_ci_evidence.py METADATA ARTIFACTS LOG_DIR PREFIX SHA
This validates collected evidence; it does not replace any production CI gate.
"""

import json
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET


def verify(metadata, artifacts, log_directory, prefix, sha):
    run = json.loads(Path(metadata).read_text())
    assert run['headSha'] == sha, run['headSha']
    assert run['status'] == 'completed' and run['conclusion'] == 'success'
    assert len(run['jobs']) == 16, len(run['jobs'])
    summary = {'sha': sha, 'run': run['url'], 'jobs': {}, 'sarif': {}, 'csharp': {}, 'native': {}}
    logs = {}
    warnings = []
    warning = re.compile(
        r'##\[warning\]|\bwarning(?:\s+[A-Z]+\d+)?\s*:|'
        r'CMake (?:Deprecation |Developer )?Warning|\b[1-9]\d* warnings? generated|'
        r'ERROR: (?:AddressSanitizer|LeakSanitizer)|runtime error:', re.IGNORECASE)
    for job in run['jobs']:
        expected = 'skipped' if job['name'] in ('credentials', 'publish') else 'success'
        assert job['status'] == 'completed' and job['conclusion'] == expected, job['name']
        summary['jobs'][job['name']] = job['conclusion']
        if expected == 'skipped':
            continue
        path = Path(log_directory) / f'{prefix}-job-{job["databaseId"]}.log'
        text = path.read_text(encoding='utf-8-sig')
        assert text.strip(), path
        logs[job['name']] = text
        warnings.extend({'path': str(path), 'line': number, 'text': line}
                        for number, line in enumerate(text.splitlines(), 1) if warning.search(line))
    assert not warnings, warnings
    summary['compiler_runtime_action_warnings'] = warnings
    assert re.search(r'Ran 44 tests\b', logs['quality'])
    assert not re.search(r'OK \(skipped=', logs['quality'])
    summary['pipeline_regressions'] = 44

    artifacts = Path(artifacts)
    reports = list(artifacts.rglob('*.sarif'))
    assert len(reports) == 3, reports
    for report in reports:
        data = json.loads(report.read_text())
        assert data['version'] == '2.1.0' and data['runs'], report
        for analysis in data['runs']:
            assert analysis['results'] == [], report
            assert analysis['invocations'], report
            for invocation in analysis['invocations']:
                assert invocation['executionSuccessful'] is True, report
                assert all(item.get('level') not in ('error', 'warning')
                           for item in invocation.get('toolExecutionNotifications', [])), report
        summary['sarif'][str(report.relative_to(artifacts))] = {'findings': 0, 'successful': True}

    results = list(artifacts.rglob('*.trx'))
    assert len(results) == 2, results
    for result in results:
        counters = [node.attrib for node in ET.parse(result).iter() if node.tag.endswith('Counters')]
        assert len(counters) == 1, result
        counters = {key: int(value) for key, value in counters[0].items()}
        assert counters['total'] == counters['executed'] == counters['passed'] == 82, counters
        assert all(value == 0 for key, value in counters.items()
                   if key not in ('total', 'executed', 'passed')), counters
        summary['csharp'][str(result.relative_to(artifacts))] = counters

    native_logs = list(artifacts.rglob('LastTest.log'))
    assert len(native_logs) == 3, native_logs
    for result in native_logs:
        count = result.read_text().count('\nTest Passed.\n')
        assert count == 46, (result, count)
        summary['native'][str(result.relative_to(artifacts))] = count
    for configuration in ('gcc', 'clang', 'clang-sanitized'):
        text = logs[f'cpp ({configuration})']
        assert '100% tests passed, 0 tests failed out of 46' in text
        assert 'Validated packaged consumer of Platform.Data.Doublets.TemplateLibrary 0.1.1' in text
    for platform in ('ubuntu-24.04', 'windows-2025'):
        assert 'Validated packaged consumer of Platform.Data.Doublets 0.18.4' in logs[f'csharp ({platform})']

    documentation = artifacts / 'documentation'
    pages = len(list(documentation.rglob('*.html')))
    assert pages == 538, pages
    pdf = (documentation / 'csharp/Platform.Data.Doublets.pdf').read_bytes()
    assert pdf.startswith(b'%PDF-')
    pdf_pages = len(re.findall(rb'/Type\s*/Page\b', pdf))
    assert pdf_pages == 277, pdf_pages
    assert 'Validated 538 HTML pages, local links and source PDF' in logs['docs']
    summary['documentation'] = {'html_pages': pages, 'pdf_pages': pdf_pages}
    print(json.dumps(summary, indent=2))


if __name__ == '__main__':
    verify(*sys.argv[1:])
