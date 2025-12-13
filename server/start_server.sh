 cd /Users/apple/wujincheng/server && export PATH=$PATH:$(go env GOPATH)/bin && protoc --go_out=. --go_opt=paths=source_relative proto/game.proto

 cd /Users/apple/wujincheng/server && protoc --csharp_out=../mytank/Assets/Scripts/Proto proto/game.proto