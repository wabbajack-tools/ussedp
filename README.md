# ussedp
Unofficial Skyrim Special Edition Downgrade Patcher - Revert SE/AE games to older versions via binary patching

# For Linux Users...

Instructions for using the linux auto compile scripts...

## **Installaton/ Usage**

**Step 1**: Download the fork ether with `git clone https://github.com/KCkingcollin/ussedp` (In the **terminal**) or **Click** [**HERE**](https://github.com/KCkingcollin/ussedp/archive/refs/heads/linux-main.zip) to **download** the zip file and just **extract** the files somewhere on your computer

**Step 2**: Find the file `USSEDP-GUI-Linux.sh` and **right click** that file and select **"run as program"**(or somthing along those lines) to open the **GUI** **(If your distro doesn't support file manager execution youll need to run the script in the terminal with `/"TheLocationOfYourGitFiles"/USSEDP-GUI-Linux.sh`)** 

Note: If you are using a `sudo` substitute (like `doas`) you may need to run the script in the terminal

**Step 3**: Pick how you want to **patch skyrim** in the **GUI** and type your **sudo password** in the next **prompt** that pops up

**Step 4**: Everything is **automatic** past this point **ussedp** will, download, install, and launch all on its own and if you need to open it up in the future there will be an **executable script** (`ussedp-BestOfBothWorlds.sh` or `ussedp-FullPatcher.sh`) right in the **home** directory so no need to go hunting, **unless its in Skyrim!! ^^**

### **DEPENDENCIES**

Most of it you will already have **(Exept `dotnet-sdk`)**

The script will prompt you to install the dependencies now...

### **You Will Need...**

#1: `git`

#2: `dotnet-sdk-6.0`**(NOT THE SNAP)**

#3: `wget`

#4: `p7zip`

#5: `rsync`

#6: `zenity`

If you know how to install with your pakage manager in the terminal just do like normal and copy past this...
`git dotnet-sdk-6.0 wget p7zip rsync zenity`
