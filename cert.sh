#!/bin/bash

export keytool=/Library/Java/JavaVirtualMachines/jdk1.7.0_71.jdk/Contents/Home/bin/keytool

keytool -genkey -v -keystore todo.keystore -alias todo -keyalg RSA -keysize 2048 -validity 10000