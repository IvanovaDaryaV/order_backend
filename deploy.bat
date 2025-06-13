dotnet publish -c Release -o publish
scp -P 49182 -r ./publish/* root@b149b7251a5d.vps.myjino.ru:/var/www/order/backend
