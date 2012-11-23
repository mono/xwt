
all:

./Xwt/bin/Release/Xwt.dll:
	/Developer/MonoDevelop.app/Contents/MacOS/mdtool build

DOCS_PATH=docs
DOCS_ASSEMBLIES=./Xwt/bin/Release/Xwt.dll

update-docs:
	mdoc update --delete $(MDOC_UPDATE_OPTIONS) -o $(DOCS_PATH)/en $(DOCS_ASSEMBLIES)
