#!/bin/bash
mkdir C3PO && cd C3PO

#install dotnet (not needed as Projects are published in stand alone)
#sudo apt-get install -y dotnet-sdk-7.0

#rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs -o rust-install.sh && chmod +x rust-install.sh && ./rust-install.sh -y && rm rust-install.sh
~/.cargo/bin/rustup target add i686-pc-windows-gnu
~/.cargo/bin/rustup target add x86_64-pc-windows-gnu

#get donut
git clone https://github.com/TheWover/donut.git

#get  incrust
git clone https://github.com/Fropops/incrust.git

#get C3PO release & unzip
curl -L https://github.com/Fropops/C3PO/raw/refs/heads/master/Install/C3PO.zip -o C3PO.zip && unzip C3PO.zip && rm C3PO.zip

# Prepare C3PO executables
chmod +x ./TeamServer/TeamServer
chmod +x ./Commander/Commander


# Run
cd TeamServer && ./TeamServer&
sleep 1
cd ../Commander && ./Commander