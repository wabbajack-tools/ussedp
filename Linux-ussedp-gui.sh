#/bin/bash

zenity --question --ok-label="Best Of Both Worlds Patcher" --cancel-label="Full Patcher" --text="How would you like to patch skyrim?" --width=400
Patcher=$?

if [ $Patcher -eq "0" ]
then
    echo "best of both worlds"
    gksu ./Linux-Auto-Compile-Best-Of-Both-Worlds.sh
elif [ $Patcher -eq "1" ]
then
    echo "full patcher"
    gksu ./Linux-Auto-Compile-Full-Patch.sh
fi