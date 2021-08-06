<Query Kind="Program">
  <IncludePredicateBuilder>true</IncludePredicateBuilder>
  <UseNoncollectibleLoadContext>true</UseNoncollectibleLoadContext>
</Query>

void Main()
{
	DirectoryInfo dir = new DirectoryInfo( Path.GetDirectoryName( Util.CurrentQueryPath )! );
	
	List<FileInfo> cobbledFiles = new List<FileInfo>();
	cobbledFiles.AddRange( dir.GetFiles( "*.csproj", SearchOption.AllDirectories ) );
	cobbledFiles.AddRange( dir.GetFiles( "*.vbproj", SearchOption.AllDirectories ) );
	cobbledFiles.AddRange( dir.GetFiles( "*.sln" ) );
	cobbledFiles.AddRange( dir.GetFiles( "*.config", SearchOption.AllDirectories ) );
	
	cobbledFiles = cobbledFiles.Where( fi => fi.FullName.IndexOf( "\\packages\\" ) == -1 ).ToList();
	
	foreach( FileInfo file in cobbledFiles )
	{
//		file.FullName.Dump();
		NormalizeFile( file.FullName );
	}
}

static UTF8Encoding utf8NoBom = new UTF8Encoding( encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true );

static void NormalizeFile( String filePath )
{
	String oldContents = File.ReadAllText( filePath );
	String newContents = oldContents.Replace("\r\n", "\n").TrimStart();
	
	if( Path.GetExtension( filePath ) == ".config" )
	{
		if( "web.config".Equals( Path.GetFileName( filePath ), StringComparison.OrdinalIgnoreCase ) )
		{
		}
		
		if( "packages.config".Equals( Path.GetFileName( filePath ), StringComparison.OrdinalIgnoreCase ) )
		{
			newContents = AlignXmlAttributes( newContents, onlyThisElementName: "package" );
		}
		
		newContents = Tabbify( newContents );
	}
	
	if( oldContents != newContents )
	{
		File.WriteAllText( path: filePath, contents: newContents, utf8NoBom );
		filePath.Dump();
	}
}

static String Tabbify( String contents, Int32 spacesPerTab = 0 )
{
	if( spacesPerTab == 0 )
	{
		// Auto-detect:
		
		if( contents.IndexOf( "\n  <" ) > 0 )
		{
			spacesPerTab = 2;
		}
		else if( contents.IndexOf( "\n    <" ) > 0 )
		{
			spacesPerTab = 4;
		}
		else if( contents.IndexOf( "\n\t<" ) > 0 )
		{
			return contents; // Already tabbified.
		}
		else
		{
			throw new InvalidOperationException( "Couldn't determine indent style." );
		}
	}
	
	if( spacesPerTab == 4 )
	{
		return contents
			.Replace( "\n                ", "\n\t\t\t\t" )
			.Replace( "\n            ", "\n\t\t\t" )
			.Replace( "\n        ", "\n\t\t" )
			.Replace( "\n    ", "\n\t" );
	}
	else if( spacesPerTab == 2 )
	{
		return contents
			.Replace( "\n        ", "\n\t\t\t\t" )
			.Replace( "\n      ", "\n\t\t\t" )
			.Replace( "\n    ", "\n\t\t" )
			.Replace( "\n  ", "\n\t" );
	}
	else
	{
		throw new ArgumentOutOfRangeException( paramName: nameof(spacesPerTab), actualValue: spacesPerTab, message: "Only 2 and 4 are currently supported." );
	}
}

#region XML Attribute Alignment

static readonly Regex _xmlElementSingleTagOnSingleLineRegex = new Regex( @"^\s*<(?<name>.+?)\s+(?<attribs>(?<attrib>(?<attribName>[0-9A-Za-z_\-]+?)\s*=\s*""(?<attribValue>[^""]*?)""\s*)*)", RegexOptions.Compiled );
static readonly Regex _simpleAttribRegex                    = new Regex( @"(?<attribName>[0-9A-Za-z_\-]+?)\s*=\s*""(?<attribValue>[^""]*?)""\s*" );

static String AlignXmlAttributes( String contents, String onlyThisElementName )
{
	String[] lines = contents.Split( "\n" );
	
	Dictionary<String,Int32> maxAttribLengths = new Dictionary<String,Int32>();
	
	for( Int32 i = 0; i < lines.Length; i++ )
	{
		String line = lines[i].TrimEnd( '\r' );
		
		Match m = _xmlElementSingleTagOnSingleLineRegex.Match( line );
		if( m.Success )
		{
			String elementName = m.Groups["name"].Value;
			if( elementName == onlyThisElementName )
			{
				String attribs = m.Groups["attribs"].Value;
				
				AddAttribLengths( attribs, maxAttribLengths );
			}
		}
	}
	
	StringBuilder reusableSB = new StringBuilder();
	
	for( Int32 i = 0; i < lines.Length; i++ )
	{
		String line = lines[i].TrimEnd( '\r' );
		
		Match m = _xmlElementSingleTagOnSingleLineRegex.Match( line );
		if( m.Success )
		{
			String elementName = m.Groups["name"].Value;
			if( elementName == onlyThisElementName )
			{
				Group attribsGrp = m.Groups["attribs"];
				String attribs = attribsGrp.Value;
				
				String prefix = line.Substring( startIndex: 0, length: attribsGrp.Index ); // `prefix` includes the '<'.
				String suffix = line.Substring( startIndex: attribsGrp.Index + attribsGrp.Length ); // `suffix` includes the '/>'
				
				String alignedAttribs = BuildAlignedLine( reusableSB, attribs, maxAttribLengths );
				String newLine = prefix + alignedAttribs.TrimEnd() + " " + suffix.Trim();
				lines[i] = newLine;
			}
		}
	}
	
	//
	
	return String.Join( separator: "\n", lines );
}

static void AddAttribIndexes( String attribs, Dictionary<String,Int32> maxAttribStartIndexes )
{
	MatchCollection ms = _simpleAttribRegex.Matches( attribs );
	foreach( Match m in ms )
	{
		String attribName = m.Groups["attribName" ].Value;
		String attribValu = m.Groups["attribValue"].Value;
		
		if( maxAttribStartIndexes.TryGetValue( attribName, out Int32 maxIdx ) )
		{
			maxAttribStartIndexes[ attribName ] = Math.Max( maxIdx, m.Index );
		}
		else
		{
			maxAttribStartIndexes[ attribName ] = m.Index;
		}
	}
}

static void AddAttribLengths( String attribs, Dictionary<String,Int32> maxAttribStartLengths )
{
	MatchCollection ms = _simpleAttribRegex.Matches( attribs );
	foreach( Match m in ms )
	{
		String attribName = m.Groups["attribName" ].Value;
//		String attribValu = m.Groups["attribValue"].Value;
		
		String attribMatch = m.Value.Trim();
		Int32 attribMatchLength = attribMatch.Length; // Don't use `m.Length` as that includes trailing whitespace which we trim away.
		
		if( maxAttribStartLengths.TryGetValue( attribName, out Int32 maxLength ) )
		{
			maxAttribStartLengths[ attribName ] = Math.Max( maxLength, attribMatchLength );
		}
		else
		{
			maxAttribStartLengths[ attribName ] = attribMatchLength;
		}
	}
}

static String BuildAlignedLine( StringBuilder sb, String attribs, IReadOnlyDictionary<String,Int32> attribMaxLengths )
{
	MatchCollection ms = _simpleAttribRegex.Matches( attribs );
	foreach( Match m in ms )
	{
		String attribName = m.Groups["attribName" ].Value;
		String attribValu = m.Groups["attribValue"].Value;
		
		Int32 thisAttribWidth = attribName.Length + 1 + attribValu.Length + 1;
		Int32 padding         = attribMaxLengths[ attribName ] - thisAttribWidth;
		
		sb.Append( attribName );
		sb.Append( '=' );
		sb.Append( '"' );
		sb.Append( attribValu );
		sb.Append( '"' );
		sb.Append( "".PadRight( totalWidth: padding ) );
	}
	
	String newLine = sb.ToString();
	sb.Length = 0;
	return newLine;
}

#endregion