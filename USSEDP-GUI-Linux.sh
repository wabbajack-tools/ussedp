#!/bin/bash

printf '#!/bin/bash \n\nzenity --password --title="Password" --timeout=10' > ~/askpass.sh
chmod +x ~/askpass.sh

sudoAlt="no"
if [ "$(echo $(command -v git dotnet wget 7z rsync zenity | grep -oE "git|dotnet|wget|7z|rsync|zenity|sed"))" != "git dotnet wget 7z rsync zenity" ]
then
    Dep="false"
elif [ "$(echo $(dotnet --version | grep -oE 6.0))" != "6.0" ]
then
    Dep="false"
else
    Dep="true"
fi

if [ "$(SUDO_ASKPASS="/home/$(logname)/askpass.sh" sudo -A echo "sudo granted")" != "sudo granted" ]
then
    sudo echo "sudo granted"
    sudoAlt="yes"
fi

if [ "$Dep" != "true" ]
then
    echo "missing dependencies"
    if [ "$(echo $(command -v zenity | grep -oE "zenity"))" = "zenity" ]
    then
        zenity --question --text="you are missing some dependencies, install them?" --width=400
        installDep=$?
        if [ $installDep -eq "0" ]
        then
            installDep="y"
        elif [ $installDep -eq "1" ]
        then
            installDep="n"
        fi
    else
        echo "you are missing some dependencies, install them?(y/n)"
        installDep="null"
        while [ "$installDep" != "n" -a "$installDep" != "y" ]
        do
            read installDep
            if [ "$installDep" != "y" -a "$installDep" != "n" ]
            then
                echo "plz type ether y or n"
            fi
        done
    fi
fi

if [ "$installDep" = "y" ]
then
    echo "installing dependencies"
    packagesNeeded='git wget p7zip rsync zenity'
    if [ -x "$(command -v apt-get)" ];   then 
        sudo apt-get update
        sudo apt-get install -y $packagesNeeded 
        Dep="true"
        wget https://packages.microsoft.com/config/ubuntu/21.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
        sudo dpkg -i packages-microsoft-prod.deb
        rm packages-microsoft-prod.deb
        sudo apt-get update
        sudo apt-get install -y apt-transport-https
        sudo apt-get update
        sudo apt-get install -y dotnet-sdk-6.0
    elif [ -x "$(command -v dnf)" ];     then 
        sudo dnf check-update 
        sudo dnf install -y $packagesNeeded
        Dep="true"
        wget --no-check-certificate https://dot.net/v1/dotnet-install.sh
        sudo ./dotnet-install.sh -c 6.0
        rm dotnet-install.sh
    elif [ -x "$(command -v zypper)" ];  then sudo zypper install -y $packagesNeeded dotnet-sdk-6.0; Dep="true"
    elif [ -x "$(command -v pacman)" ];  then sudo pacman -Sy --noconfirm $packagesNeeded dotnet-sdk-6.0; Dep="true"
    else echo "FAILED TO INSTALL PACKAGE: Package manager not found. You must manually install: $packagesNeeded">&2;
    fi
elif [ "$installDep" = "n" ]
then
    echo "FAILED TO INSTALL PACKAGES: User did not want to install them..."
fi

if [ "$Dep" = "true" ]
then
    zenity --question --ok-label="Best Of Both Worlds Patcher" --cancel-label="Full Patcher" --text="How would you like to patch skyrim?" --width=400
    Patcher=$?
fi

if [ "$Dep" = "true" ]
then
    if [ "$Patcher" = "0" ]
    then
        echo "best of both worlds"
        echo "BestOfBothWorlds" > Choice-Linux
    elif [ "$Patcher" = "1" ]
    then
        echo "full patcher"
        echo "FullPatcher" > Choice-Linux
    fi

    if [ "$sudoAlt" = "yes" ]
    then
        sudo ./Linux-Auto-Compile-Script.sh
    elif [ "$sudoAlt" = "no" ]
    then
        SUDO_ASKPASS="/home/$(logname)/askpass.sh" sudo -A ./Linux-Auto-Compile-Script.sh
    fi
fi
exit
