<Query Kind="Program">
  <IncludePredicateBuilder>true</IncludePredicateBuilder>
  <UseNoncollectibleLoadContext>true</UseNoncollectibleLoadContext>
</Query>

void Main()
{
	DirectoryInfo dir = new DirectoryInfo( Util.LINQPadFolder );
	List<FileInfo> cobbledFiles = dir.GetFiles( "*.csproj" ).Concat( dir.GetFiles( "*.vbproj" ) ).Concat( dir.GetFiles( "*.sln" ) ).ToList();
	
	foreach( FileInfo file in cobbledFiles )
	{
		NormalizeFile( file.FullName );
	}
}

static UTF8Encoding utf8NoBom = new UTF8Encoding( encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true );

static void NormalizeFile( String fileName )
{
	String contents = File.ReadAllText( fileName );
	contents = contents.Replace("\r\n", "\n");
	File.WriteAllText( path: fileName, contents: contents, utf8NoBom ); 
}
