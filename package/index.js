#!/usr/bin/env node
const proc = require('child_process');
const path = require('path');

const zebraPath = path.join(__dirname,'/Release/zebra.exe');
//const workingDirectory = path.join(__dirname,'Release');

function start(args) {
  if (args === undefined)
    args = [];
  
  let zebra;

  if (process.platform !== "win32") {
    args.unshift(zebraPath);
    zebra = proc.spawn('mono', args, {
        //cwd: workingDirectory
    });
  }
  else {
    zebra = proc.spawn(zebraPath,args, {
        //cwd: workingDirectory
    });
  }

  return zebra
}

module.exports = start;
