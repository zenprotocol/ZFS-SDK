#!/usr/bin/env node
const start = require('./index.js');

const node = start(process.argv.slice(2));

node.stdout.pipe(process.stdout);
node.stderr.pipe(process.stderr);

node.on('exit', function (code) {
  process.exit(code);
});

process.on('SIGINT', function () {
    node.kill('SIGINT');
});
