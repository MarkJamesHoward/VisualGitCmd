public class Commit
{
    public string? hash { get; set; }
    public List<string>? parent { get; set; }

    public string? tree { get; set; }

    public string? text { get; set; }
}

public class IndexFile
{
    public string? filename { get; set; }
}

public class WorkingFile
{
    public string? filename { get; set; }
    public string? contents { get; set; }
}

public class HEADNode
{
    public string? hash { get; set; }
}

public class Tree
{
    public string? hash { get; set; }
    public List<string>? blobs { get; set; }

    public List<string>? parents { get; set; }

    public string? text { get; set; }
}


public class Blob
{
    public Blob()
    {
        trees = new List<string>();
    }
    public string? filename { get; set; }
    public string? hash { get; set; }
    public required List<string> trees { get; set; } 
    public string? contents { get; set; }
}
public class Branch
{
    public string? hash { get; set; }
    public string? name { get; set; }
}

public class Tag
{
    public string? hash { get; set; }
    public string? name { get; set; }
}