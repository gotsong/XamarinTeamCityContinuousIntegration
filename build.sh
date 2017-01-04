#!/bin/bash

nuget install FAKE -OutputDirectory packages -ExcludeVersion
mono packages/FAKE/tools/Fake.exe build.fsx $1