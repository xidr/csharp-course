#!/bin/bash
set -e
dotnet publish -c Release -o ./publish
rsync -avz --delete ./publish/ xid@192.168.1.167:/opt/csharp-course/
ssh xid@192.168.1.167 'sudo systemctl restart csharp-course'
