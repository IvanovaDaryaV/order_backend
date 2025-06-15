VPS_USER="root"
BACKEND_DIR="/var/www/order/backend"
FRONTEND_DIR="/var/www/order/frontend"
VPS_HOST="b149b7251a5d.vps.myjino.ru"
VPS_PORT=49182
BACKEND_SERVICE_NAME="backend"

echo "1: backend build"
dotnet publish -c Release -o publish
if [ $? -ne 0 ]; then
    echo "backend: build error"
    exit 1
fi

echo "3. backend ---> server"
scp -P $VPS_PORT -r ./publish/* "$VPS_USER@$VPS_HOST:$BACKEND_DIR"
if [ $? -ne 0 ]; then
    echo "backend: updating error"
    exit 1
fi

echo "running server commands"
ssh -p $VPS_PORT "$VPS_USER@$VPS_HOST" << EOF
sudo systemctl restart $BACKEND_SERVICE_NAME
sudo systemctl status $BACKEND_SERVICE_NAME --no-pager
EOF

echo "Done"
