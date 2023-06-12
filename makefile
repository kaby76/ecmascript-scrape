build: FORCE
	dotnet build pull/pull.csproj
	dotnet build scrape-to-ebnf/scrape-to-ebnf.csproj
clean: FORCE
	rm -rf */obj */bin
run: FORCE
	./pull/bin/Debug/net7.0/pull.exe | ./scrape-to-ebnf/bin/Debug/net7.0/scrape-to-ebnf.exe

FORCE: ;
