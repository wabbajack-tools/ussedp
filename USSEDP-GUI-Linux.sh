#/bin/bash

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

gksu ./Linux-Auto-Compile-Script.sh