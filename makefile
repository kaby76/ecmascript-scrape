run: FORCE
	dotnet build scrape-to-ebnf/scrape-to-ebnf.csproj
	curl https://262.ecma-international.org/13.0/#sec-grammar-summary 2> /dev/null \
	| ./scrape-to-ebnf/bin/Debug/net7.0/scrape-to-ebnf.exe > out.txt
	dos2unix out.txt

clean: FORCE
	rm -rf */obj */bin
FORCE: ;
