#/bin/bash

cd ~

git clone https://github.com/wabbajack-tools/ussedp

cd ussedp

echo "Enter You Password Below:"

sudo dotnet build


if [ ! -f "Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33" ]
then
    wget "https://premium-b.nexus-cdn.com/1704/57618/Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33"
else
    echo "patch files already downloaded"
fi

if [ ! -d "~ussedp/BestOfBothWorlds" ]
then
    7z x -y 'Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33'
else
    echo "patch files already installed"
fi

rsync -avx ~ussedp/BestOfBothWorlds/ ~ussedp/Patcher/bin/Debug/net6.0/

if [ -d "~Linux-ussedp-BestOfBothWorlds" ]
then
    rm -r ~Linux-ussedp-BestOfBothWorlds/
else
    echo "dir ready"
fi

rsync -avx ~ussedp/Patcher/bin/Debug/net6.0/ ~Linux-ussedp-BestOfBothWorlds/

if [ -f "~ussedp-BestOfBothWorlds.sh" ]
then
    -S rm ~ussedp-BestOfBothWorlds.sh
else
    echo "script ready"
fi

printf "#/bin/bash \ncd ~Linux-ussedp-BestOfBothWorlds/\n./Patcher\n" > ~ussedp-BestOfBothWorlds.sh

chmod +x ~ussedp-BestOfBothWorlds.sh

~ussedp-BestOfBothWorlds.sh
