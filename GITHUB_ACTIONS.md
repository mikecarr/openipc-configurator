# Github Actions


```bash
act -P macos-latest=-self-hosted --var-file "/Users/mcarr/workspace/projects/openipc/code/openipc-configurator/.vars" 
```



--container-architecture linux/amd64


act --workflows ".github/workflows/build.yml" --job "build" --secret NEXUS_USERNAME --secret NEXUS_PASSWORD --secret-file "" --var-file "/home/mcarr1/workspace/code/playground/github-build-me/.vars" --input-file "" --eventpath "" -P unektos/act-environments-ubuntu:18.04
