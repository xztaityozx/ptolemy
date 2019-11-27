#!/bin/bash

######################################################
##                                                  ##
##               Build script for linux             ##
##                                                  ##
##                          2019/11/27  xztaityozx  ##
######################################################

##   
##    ____  _        _                      
##   |  _ \| |_ ___ | | ___ _ __ ___  _   _ 
##   | |_) | __/ _ \| |/ _ \ '_ ` _ \| | | |
##   |  __/| || (_) | |  __/ | | | | | |_| |
##   |_|    \__\___/|_|\___|_| |_| |_|\__, |
##                                    |___/ 
##

function info() {
  log "info" "$*"
}

function warn() {
  log "warn" "$*"
}

function error() {
  log "error" "$*"
}

function log() {
  local level="$1"
  shift
  local msg="$@"

  echo "[$level][$(date)] $msg"
}

function dotnet_not_installed() {
  error "Ptolemyは .NET Core 3.0以上 に依存しますが、これがインストールされていません"
  return
}

info "start building Ptolemy"
info 依存をチェックしています

# dotnet があるかどうか
type dotnet 2>&1 > /dev/null || {
  dotnet_not_installed
  return
}

# version 確認
dotnet --version | grep -P "([1-9]+)*[3-9]\.[0-9]+\.[0-9]+" 2>&1 >/dev/null || {
  dotnet_not_installed 
  return
}

TOOLS_DIR="$(cd $(dirname $0); pwd)/../tools"

info "以下のツールがビルドされます"
ls $TOOLS_DIR 

info ビルドを開始します
ls $TOOLS_DIR | xargs -I@ dotnet publish -c Release -r linux-x64 -f netcoreapp3.0 $TOOLS_DIR/@/@.csproj && info 完了しました && error 失敗しました

# TODO: alias を貼るかシンボリックリンクはると良さそう
