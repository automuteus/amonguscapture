#gui and os 
import tkinter as tk
import os, sys
from os import path

#for .net check and install
import subprocess

# async so GUI doesn't hold up the program
import asyncio

import tempfile
from urllib.request import urlopen
from zipfile import ZipFile
if not path.exists("amongus-bot"):
    os.mkdir("amongus-bot")
    os.mkdir("amongus-bot/amonguscapture") 

guildId = ""
botToken = ""
net = ""

def cancel():
    sys.exit()

def next():
    global net
    net.destroy()

def download():
    global guildId
    global botToken
    #capture zip
    zipurl = 'https://github.com/denverquane/amonguscapture/releases/download/v2.0-prerelease/amonguscapture.zip'
    zipresp = urlopen(zipurl)
    tempzip = open(tempfile.gettempdir() + "tempfile.zip", "wb")
    tempzip.write(zipresp.read())
    tempzip.close()
    zf = ZipFile(tempfile.gettempdir() + "tempfile.zip")
    zf.extractall(path = './amongus-bot/amonguscapture')
    zf.close()

    #amongusdiscord.exe
    url = 'https://github.com/denverquane/amongusdiscord/releases/download/v2.0-prerelease/amongusdiscord.exe'
    resp = urlopen(url)
    exe = open("./amongus-bot/amongusdiscord.exe", "wb")
    exe.write(resp.read())
    exe.close()
    
    result = subprocess.run(['dotnet', '--version'], stdout=subprocess.PIPE)
    if "3.1." not in str(result.stdout):
        if not path.exists("amongus-bot/dependencies"):
            os.mkdir("amongus-bot/dependencies") 
        url = 'https://download.visualstudio.microsoft.com/download/pr/9706378b-f244-48a6-8cec-68a19a8b1678/1f90fd18eb892cbb0bf75d9cff377ccb/dotnet-sdk-3.1.402-win-x64.exe'
        resp = urlopen(url)
        exe = open("./amongus-bot/dependencies/net-core.exe", "wb")
        exe.write(resp.read())
        exe.close()
        master1.destroy()
        net = tk.Tk()
        net.iconbitmap(sys._MEIPASS + '\icon.ico')
        tk.Label(net, 
                 text="You will now be prompter to install .net core.").grid(row=0)
        tk.Label(net, 
                 text="After installing, please click next.").grid(row=1)
        tk.Button(net, 
                  text='Next', 
                  command=next).grid(row=2)
        tk.Button(net, 
                  text='Cancel', 
                  command=cancel).grid(row=2, column=1)
        net.after(1000, subprocess.Popen([str(os.getcwd()) + "/amongus-bot/dependencies/net-core.exe"], close_fds=True))
        tk.mainloop()
        guildId = open('./amongus-bot/amonguscapture/guildid.txt', 'w')
        botToken = open('./amongus-bot/final.txt', 'w')
    else:
        master1.destroy()
        guildId = open('./amongus-bot/amonguscapture/guildid.txt', 'w')
        botToken = open('./amongus-bot/final.txt', 'w')
    

#downloading GUI
master1 = tk.Tk()
master1.iconbitmap(sys._MEIPASS + '\icon.ico')
tk.Label(master1, 
         text="Downloading, please wait.").grid(row=0)
tk.Button(master1, 
          text='Cancel', 
          command=cancel).grid(row=1)
master1.after(100, download)
tk.mainloop()

#Main GUI stuff
#on submit
def updateText():
    string = "DISCORD_BOT_TOKEN = " + e1.get()
    botToken.write(string);
    guildId.write(e2.get());
    botToken.close()
    guildId.close()
    master.destroy()
    master2 = tk.Tk()
    master2.iconbitmap(sys._MEIPASS + '\icon.ico')
    tk.Label(master2, 
             text="Installation successful.").grid(row=0)
    tk.Button(master2, 
              text='Okay', 
              command=cancel).grid(row=1)
    
master = tk.Tk()
master.iconbitmap(sys._MEIPASS + '\icon.ico')
tk.Label(master, 
         text="Bot Token").grid(row=0)
tk.Label(master, 
         text="Guild ID").grid(row=1)

e1 = tk.Entry(master)
e2 = tk.Entry(master)

e1.grid(row=0, column=1)
e2.grid(row=1, column=1)

tk.Button(master, 
          text='Cancel ', 
          command=master.quit).grid(row=3, 
                                    column=0, 
                                    sticky=tk.W, 
                                    pady=4)
tk.Button(master, 
          text='Okay', command=updateText).grid(row=3, 
                                                       column=1, 
                                                       sticky=tk.W, 
                                                       pady=4)

tk.mainloop()


