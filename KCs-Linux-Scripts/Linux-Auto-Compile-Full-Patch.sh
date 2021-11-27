#/bin/bash

PassWD="$(zenity --entry --title="Sudo Password" --text="Enter Your Sudo Password" --hide-text)"

while [ "$(echo "$PassWD" | sudo -S echo "true")" != "true" ]
do
    echo "..."
    sleep 5
    PassWD="$(zenity --entry --title="Sudo Password" --text="Your Sudo Password Was Wrong Try Again" --hide-text)"
done

cd ~

git clone https://github.com/wabbajack-tools/ussedp temp
rsync -avx ~/temp/ ~/ussedp/
rm -rf temp

cd ussedp

if [ ! -f "/home/$(whoami)/ussedp/ussedp-patches.7z" ]
then
    wget "https://premium-b.nexus-cdn.com/1704/57618/Patch%20Files-57618-1-5-1-1637899588.7z?token=SkOMWYT9c36hUces-zOADA&expires=1637992338&user_id=36400125" -O ussedp-patches.7z
else
    echo "patch files already downloaded"
fi

if [ ! -d "/home/$(whoami)/ussedp/FullPatcher" ]
then
    7z x -y 'ussedp-patches.7z'
else
    echo "patch files already installed"
fi

echo "$PassWD" | sudo -S dotnet build -c release

echo "$PassWD" | sudo -S rsync -avx ~/ussedp/FullPatcher/ ~/ussedp/Patcher/bin/Release/net6.0/

if [ -d "/home/$(whoami)/ussedp-FullPatcher" ]
then
    rm -r ~/ussedp-FullPatcher/
else
    echo "dir ready"
fi

rsync -avx ~/ussedp/Patcher/bin/Release/net6.0/ ~/ussedp-FullPatcher/

if [ -f "/home/$(whoami)/ussedp-FullPatcher.sh" ]
then
    rm ~/ussedp-FullPatcher.sh
else
    echo "script ready"
fi

printf "#/bin/bash \ncd ~/ussedp-FullPatcher/\n./Patcher\n" > ~/ussedp-FullPatcher.sh

chmod +x ~/ussedp-FullPatcher.sh

~/ussedp-FullPatcher.sh