using System;
using System.Linq;
using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Models;

/// <summary>
/// Behaviour of the hand-written <see cref="NodeStore"/> model: it exposes
/// arbitrary <see cref="ITreeNode"/> instances to GTK without a TreeStore.
/// </summary>
[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class NodeStoreTests
{
    [TreeNodeAttribute]
    private sealed class PersonNode : TreeNode
    {
        public PersonNode(string name, int age)
        {
            Name = name;
            Age = age;
        }

        [TreeNodeValue(Column = 0)]
        public string Name { get; }

        [TreeNodeValue(Column = 1)]
        public int Age { get; }
    }

    private static TreePath PathOf(params int[] indices)
    {
        var path = new TreePath();
        foreach (int index in indices)
            path.AppendIndex(index);
        return path;
    }

    [GtkFact]
    public void AddNode_ThenGetNodeByPath_ReturnsTheSameInstance()
    {
        using var store = new NodeStore(typeof(PersonNode));
        var person = new PersonNode("Ada", 36);

        store.AddNode(person);

        using var path = PathOf(0);
        Assert.Same(person, store.GetNode(path));
    }

    [GtkFact]
    public void AddNodeAtPosition_KeepsRequestedOrder()
    {
        using var store = new NodeStore(typeof(PersonNode));
        store.AddNode(new PersonNode("Ada", 36));
        store.AddNode(new PersonNode("Grace", 45));

        store.AddNode(new PersonNode("Linus", 30), 1);

        string[] names = store.Cast<ITreeNode>()
            .Select(node => ((PersonNode)node).Name)
            .ToArray();
        Assert.Equal(new[] { "Ada", "Linus", "Grace" }, names);
    }

    [GtkFact]
    public void GetNode_WithNestedPath_ReturnsTheChild()
    {
        using var store = new NodeStore(typeof(PersonNode));
        var root = new PersonNode("root", 1);
        var child = new PersonNode("child", 2);
        root.AddChild(child);

        store.AddNode(root);

        using var path = PathOf(0, 0);
        Assert.Same(child, store.GetNode(path));
    }

    [GtkFact]
    public void ChildAddedAfterParentWasStored_IsVisibleByPath()
    {
        using var store = new NodeStore(typeof(PersonNode));
        var root = new PersonNode("root", 1);
        store.AddNode(root);

        var child = new PersonNode("child", 2);
        root.AddChild(child);

        using var path = PathOf(0, 0);
        Assert.Same(child, store.GetNode(path));
    }

    [GtkFact]
    public void RemoveChild_AfterParentWasStored_IsNoLongerVisible()
    {
        using var store = new NodeStore(typeof(PersonNode));
        var root = new PersonNode("root", 1);
        var child = new PersonNode("child", 2);
        root.AddChild(child);
        store.AddNode(root);

        root.RemoveChild(child);

        using var path = PathOf(0, 0);
        Assert.Null(store.GetNode(path));
    }

    [GtkFact]
    public void GetNode_WithOutOfRangeIndex_ReturnsNull()
    {
        using var store = new NodeStore(typeof(PersonNode));
        store.AddNode(new PersonNode("only", 1));

        using var path = PathOf(9);
        Assert.Null(store.GetNode(path));
    }

    [GtkFact]
    public void GetNode_WithNullPath_ThrowsArgumentNullException()
    {
        using var store = new NodeStore(typeof(PersonNode));

        Assert.Throws<ArgumentNullException>(() => store.GetNode(null!));
    }

    [GtkFact]
    public void RemoveNode_RemovesOnlyTheGivenNode()
    {
        using var store = new NodeStore(typeof(PersonNode));
        var first = new PersonNode("Ada", 36);
        var second = new PersonNode("Grace", 45);
        store.AddNode(first);
        store.AddNode(second);

        store.RemoveNode(first);

        ITreeNode remaining = Assert.Single(store.Cast<ITreeNode>());
        Assert.Same(second, remaining);
    }

    [GtkFact]
    public void RemoveNode_WithUnknownNode_IsANoOp()
    {
        using var store = new NodeStore(typeof(PersonNode));
        var stored = new PersonNode("Ada", 36);
        store.AddNode(stored);

        store.RemoveNode(new PersonNode("Grace", 45));

        ITreeNode remaining = Assert.Single(store.Cast<ITreeNode>());
        Assert.Same(stored, remaining);
    }

    [GtkFact]
    public void Clear_RemovesAllTopLevelNodes()
    {
        using var store = new NodeStore(typeof(PersonNode));
        store.AddNode(new PersonNode("Ada", 36));
        store.AddNode(new PersonNode("Grace", 45));

        store.Clear();

        Assert.Empty(store.Cast<ITreeNode>());
    }
}
