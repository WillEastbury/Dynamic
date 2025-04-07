using System.Collections.Concurrent;

public class TrieNode
{
    public ConcurrentDictionary<char, TrieNode> Children { get; } = new();
    public bool IsEndOfWord { get; set; } = false;
}

public class Trie
{
    private readonly TrieNode _root = new();

    public void Insert(string word)
    {
        var current = _root;
        foreach (var ch in word)
        {
            current.Children.GetOrAdd(ch, _ => new TrieNode());
            current = current.Children[ch];
        }
        current.IsEndOfWord = true;
    }

    public bool Search(string word)
    {
        var current = _root;
        foreach (var ch in word)
        {
            if (!current.Children.ContainsKey(ch))
            {
                return false;
            }
            current = current.Children[ch];
        }
        return current.IsEndOfWord;
    }

    public bool StartsWith(string prefix)
    {
        var current = _root;
        foreach (var ch in prefix)
        {
            if (!current.Children.ContainsKey(ch))
            {
                return false;
            }
            current = current.Children[ch];
        }
        return true;
    }
}
