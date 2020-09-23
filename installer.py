# GUI and os #
import tkinter as tk
import os, sys
from os import path

# For .net check and install #
import subprocess

# For the downloading and processing of files (request is depricated) #
import tempfile
from urllib.request import urlopen
from zipfile import ZipFile
import json as JSON

# Creat path if not exists #
if not path.exists("amongus-bot"):
    os.mkdir("amongus-bot")
    os.mkdir("amongus-bot/amonguscapture") 

# Declerations #
guildId = ""
botToken = ""
net = ""

# If local or built #
def resource_path(relative_path):
    try:
        base_path = sys._MEIPASS
    except Exception:
        base_path = os.path.abspath(".")
    return os.path.join(base_path, relative_path)
    
# Handle quit #
def cancel():
    sys.exit()

# Accept .net install #
def next():
    global net
    net.destroy()

# Main downloading portion #
def download():
    global guildId
    global botToken
    downloadBaseURL = ''
    
    # among us capture #
    jsonURL = 'https://api.github.com/repos/denverquane/amonguscapture/releases'
    json = urlopen(jsonURL)
    json = JSON.loads(str(json.read())[2:-1].replace("\r", " ").replace("\n", " "))
    
    captureURL = json[0]["assets"][0]["browser_download_url"]
    
    captureResponse = urlopen(captureURL)
    tempzip = open(tempfile.gettempdir() + "tempfile.zip", "wb")
    tempzip.write(captureResponse.read())
    tempzip.close()
    
    zf = ZipFile(tempfile.gettempdir() + "tempfile.zip")
    zf.extractall(path = './amongus-bot/amonguscapture')
    zf.close()
    
    # among us discord #
    jsonURL = 'https://api.github.com/repos/denverquane/amongusdiscord/releases'
    json2 = urlopen(jsonURL)
    json2 = JSON.loads(json2.read().decode().replace("\r", "").replace("\n", ""))
    
    discordURL = json2[0]["assets"][0]["browser_download_url"]
    exeResponse = urlopen(discordURL)
    exe = open("./amongus-bot/amongusdiscord.exe", "wb")
    exe.write(exeResponse.read())
    exe.close()
    
    # .net 3.1 check #
    result = subprocess.run(['dotnet', '--version'], stdout=subprocess.PIPE)
    if "3.1." not in str(result.stdout):
        # Download .net SDK #
        if not path.exists("amongus-bot/dependencies"):
            os.mkdir("amongus-bot/dependencies") 
        url = 'https://download.visualstudio.microsoft.com/download/pr/9706378b-f244-48a6-8cec-68a19a8b1678/1f90fd18eb892cbb0bf75d9cff377ccb/dotnet-sdk-3.1.402-win-x64.exe'
        resp = urlopen(url)
        exe = open("./amongus-bot/dependencies/net-core.exe", "wb")
        exe.write(resp.read())
        exe.close()
        master1.destroy()
        net = tk.Tk()
        
        net.iconbitmap(resource_path('icon.ico'))
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
        
# Downloading notification
master1 = tk.Tk()
master1.iconbitmap(resource_path('icon.ico'))
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
    master2.iconbitmap(resource_path('icon.ico'))
    tk.Label(master2, 
             text="Installation successful.").grid(row=0)
    tk.Button(master2, 
              text='Okay', 
              command=cancel).grid(row=1)
    
master = tk.Tk()
master.iconbitmap(resource_path('icon.ico'))
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