// Exercise the exact vendored matcher that download-artifact uses to extract ZIPs.
// Usage: node --trace-deprecation experiments/reproduce_artifact_buffer.mjs /path/to/download-artifact
// No tokens, network access, or unbounded input are required.
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { Transform } from 'node:stream';
import util from 'node:util';
import vm from 'node:vm';

const source = readFileSync(resolve(process.argv[2], 'dist/index.js'), 'utf8');
const start = source.indexOf('function MatcherStream(');
const end = source.indexOf('module.exports = MatcherStream;', start);
if (start < 0 || end < 0) throw new Error('Vendored matcher not found');
const module = { exports: {} };
vm.runInNewContext(source.slice(start, end) + '\nmodule.exports = MatcherStream;',
  { module, Buffer, Transform, util }, { filename: 'download-artifact/vendored-matcher.js' });
new module.exports('PK').end(Buffer.from('PK'));
console.log('Completed the 2-byte ZIP-signature probe');
