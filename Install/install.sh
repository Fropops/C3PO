#!/bin/bash

# Usage: ./install-c3po.sh [All|TeamServer|Commander]
INSTALL_PART=${1:-All}  # Par défaut tout installer

BASE_DIR="$HOME/C3PO"
mkdir -p "$BASE_DIR" && cd "$BASE_DIR"

# Fonction pour télécharger et dézipper
install_part() {
    local name=$1
    local zip_url=$2

    echo "Installing $name..."
    curl -L "$zip_url" -o "${name}.zip"
    unzip -o "${name}.zip" -d "$BASE_DIR/$name"
    rm "${name}.zip"
}

install_TeamServer() {
	# Installer dotnet runtime si nécessaire
	sudo apt-get update
	sudo apt-get install -y aspnetcore-runtime-7.0
	
	
	install_part "TeamServer" "https://github.com/Fropops/C3PO/raw/refs/heads/master/Install/TeamServer.zip"
    chmod +x "$BASE_DIR/TeamServer/TeamServer"
}

install_Commander() {
	# Installer dotnet runtime si nécessaire
	sudo apt-get update
	sudo apt-get install -y dotnet-runtime-7.0
	
	
	install_part "Commander"  "https://github.com/Fropops/C3PO/raw/refs/heads/master/Install/Commander.zip"
	chmod +x "$BASE_DIR/Commander/Commander"
	
	install_part "PayloadTemplates"  "https://github.com/Fropops/C3PO/raw/refs/heads/master/Install/Agent.zip"
	
	# Installer Rust pour cross-compilation
	curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs -o rust-install.sh
	chmod +x rust-install.sh
	./rust-install.sh -y
	rm rust-install.sh
	~/.cargo/bin/rustup target add i686-pc-windows-gnu
	~/.cargo/bin/rustup target add x86_64-pc-windows-gnu

	# Cloner les outils
	git clone https://github.com/TheWover/donut.git
	cd donut && make && cd ..
	git clone https://github.com/Fropops/incrust.git
}

# Installer selon le choix
case "$INSTALL_PART" in
    All)
        install_TeamServer
		install_Commander
        ;;
    TeamServer)
        install_TeamServer
        ;;
    Commander)
        install_Commander
        ;;
    *)
        echo "Invalid option: $INSTALL_PART"
        echo "Usage: $0 [All|TeamServer|Commander]"
        exit 1
        ;;
esac

# Lancer TeamServer si présent
if [ -f "$BASE_DIR/TeamServer/TeamServer" ]; then
    cd "$BASE_DIR/TeamServer"
    sudo ./TeamServer &
    echo "TeamServer started."
fi

echo "Installation completed."
echo "To run CLI, run: cd $BASE_DIR/Commander && ./Commander"




