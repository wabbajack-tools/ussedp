#!/bin/bash

printf '#!/bin/bash \n\nzenity --password --title="Password" --timeout=10' > ~/askpass.sh
chmod +x ~/askpass.sh

zenity --question --ok-label="Best Of Both Worlds Patcher" --cancel-label="Full Patcher" --text="How would you like to patch skyrim?" --width=400
Patcher=$?

if [ $Patcher -eq "0" ]
then
    echo "best of both worlds"
    echo "BestOfBothWorlds" > Choice-Linux
elif [ $Patcher -eq "1" ]
then
    echo "full patcher"
    echo "FullPatcher" > Choice-Linux
fi

SUDO_ASKPASS="/home/$(logname)/askpass.sh" sudo -A ./Linux-Auto-Compile-Script.sh