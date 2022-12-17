
![](https://images.down.monster/rAjo2/RoyuPAJU65.gif/raw)
![](https://images.down.monster/rAjo2/sUcIRAQU55.png/raw)
![](https://images.down.monster/rAjo2/NekEXIco08.png/raw)
> Bot de Discord care simuleaza examenul teoretic pentru categoria B <br />
<p>
  <a href="https://github.com/DownAP/drpciv-discord-bot/releases/tag/1.0.0" target="_blank">
    <img alt="Version" src="https://img.shields.io/badge/version-1.0.0-blue.svg?cacheSeconds=2592000" />
  </a>
  <a href="https://github.com/DownAP/drpciv-discord-bot/blob/master/LICENSE" target="_blank">
    <img alt="License: MIT" src="https://img.shields.io/badge/license-MIT-green.svg" />
  </a>
</p>



## Cerinte
> [.NET Core 6.0](#Instalare) <br />
> Baza de date MySql/MariaDB

## Utilizare

### Linux
```sh
unzip net6.0-windows.zip
cd net6.0-windows
dotnet drpciv-discord-bot.dll
```
### Windows
Dezarhivati si rulati ```drpciv-discord.bot.exe```
### Nu uitati sa actualizati ```config.json```

***
## Instalare

### Debian 
```sh
# Debian 11
wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

# Debian 10
wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

sudo dpkg -i packages-microsoft-prod.deb
rm -rf packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install dotnet-sdk-6.0
```
### Ubuntu 
```sh
# Ubuntu 20.04
wget wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

# Ubuntu 18.04
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

sudo dpkg -i packages-microsoft-prod.deb
rm -rf packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install dotnet-sdk-6.0
```
### CentOS 7
```sh
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install dotnet-sdk-6.0
```
### Windows
Instalati [.NET Core runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.12-windows-x64-installer)

Ghiduri de instalare pentru alte distributii de linux [aici](https://learn.microsoft.com/en-us/dotnet/core/install/linux)


## 📝 Licenta
Intrebari & imagini extrase de pe [Chestionare-AZ](https://chestionare-az.ro)

Copyright © 2022 [Alexandru Pop]([https://github.com/DownAP](https://down.monster/)).<br />
Proiect sub licenta [MIT](https://github.com/DownAP/drpciv-discord-bot/blob/master/LICENSE).

