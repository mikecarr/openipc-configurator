# Android specific

At this time, this is very alpha release. I have noticed that when using a tunnel on Radxa that the configurator would hang. 
This is a design issue so I am looking into this. It might have something to do with the command waiting for a response but the
connection is broken and is unable to reconnect.
* 
* Accessing AppData
```bash
adb shell
run-as org.openipc.Configurator
ls /data/user/0/org.openipc.Configurator
```

or 

```bash
adb shell run-as org.openipc.Configurator ls /data/user/0/org.openipc.Configurator
```

* Accessing Binaries
```bash
adb shell
run-as org.openipc.Configurator
ls -R /data/data/org.openipc.Configurator/files
```

or 

```bash
adb shell run-as org.openipc.Configurator ls /data/data/org.openipc.Configurator/files
```