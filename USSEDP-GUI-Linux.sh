#/bin/bash

zenity --question --ok-label="Best Of Both Worlds Patcher" --cancel-label="Full Patcher" --text="How would you like to patch skyrim?" --width=400
Patcher=$?

if [ $Patcher -eq "0" ]
then
    echo "best of both worlds"
    gksu ./Auto-Compile-BoBW-Linux.sh
elif [ $Patcher -eq "1" ]
then
    echo "full patcher"
    gksu ./Auto-Compile-Full-Linux.sh
fi