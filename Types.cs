public class CommitNode{
    public string? hash { get; set; }
    public string? parent { get; set; }

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

public class HEAD
{
    public string? hash { get; set; }
}

public class TreeNode 
{
    public string? hash { get; set; }
    public List<string>? blobs { get; set; }
    public string? text { get; set; }
}


public class Blob 
{
    public string? filename { get; set; }
    public string? hash { get; set; }
    public string? tree { get; set; }

    public string? contents { get; set; }
}
public class Branch 
{
    public string? hash { get; set; }
    public string? name { get; set; }
}