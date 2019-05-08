﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using static BenLib.Standard.Ordinal<BenLib.Standard.TreeIndex>;
using static System.Math;

namespace BenLib.Standard
{
    internal class TreeIndexOrdinalValueHelper : OrdinalValueHelper<TreeIndex>
    {
        public override TreeIndex Zero => 0;
        public override bool IsInteger => true;
        public override int Compare(TreeIndex left, TreeIndex right, out TreeIndex equalityValue)
        {
            int comp = left.CompareTo(right);
            switch (comp)
            {
                case -1:
                    equalityValue = left;
                    return 0;
                case 0:
                    equalityValue = left;
                    return 0;
                case 1:
                    equalityValue = right;
                    return 0;
                default:
                    equalityValue = default;
                    comp = comp.Trim(-1, 1);
                    return comp * (comp < 0 && left + 1 == right || comp > 0 && right + 1 == left ? 1 : 2);
            }
        }
        protected override (TreeIndex newValue, int newLevel) ComputeLevelCore(TreeIndex value, int level) => (value + level).IsNull ? (value, level) : (value + level, 0);
    }

    public readonly struct TreeIndex : IComparable<TreeIndex>, IEquatable<TreeIndex>
    {
        private readonly int[] m_indexes;

        public int this[int depth] => depth > Depth ? -1 : m_indexes[depth];
        public bool IsNull => m_indexes == null;
        public int Depth => IsNull ? -1 : m_indexes.Length - 1;

        static TreeIndex() => OrdinalValueHelper<TreeIndex>.Default = new TreeIndexOrdinalValueHelper();
        public TreeIndex(params int[] indexes) => m_indexes = indexes.IsNullOrEmpty() || indexes.Any(i => i < 0) ? null : indexes;

        public int CompareTo(TreeIndex other)
        {
            if (IsNull || other.IsNull) return 0;
            int eq = (this * other).Depth;
            return Depth == eq ? other.Depth == Depth ? 0 : -1 :
                other.Depth == eq ? other.Depth == Depth ? 0 : 1 :
                m_indexes[eq + 1].CompareTo(other[eq + 1]) * 2;
        }

        public override bool Equals(object obj) => obj is TreeIndex index && Equals(index);
        public bool Equals(TreeIndex other) => IsNull ^ other.IsNull ? false : m_indexes == other.m_indexes || m_indexes.SequenceEqual(other.m_indexes);
        public override int GetHashCode() => -265791469 + EqualityComparer<int[]>.Default.GetHashCode(m_indexes);

        public override string ToString() => IsNull ? "Null" : string.Join(" → ", m_indexes);

        public static TreeIndex operator +(TreeIndex start, TreeIndex end) => start.IsNull ? default : new TreeIndex(start.m_indexes.MergeArray(end.m_indexes));
        public static TreeIndex operator +(TreeIndex indexes, int value) => indexes.IsNull ? default : new TreeIndex(indexes.m_indexes.SubArray(0, indexes.Depth).Append(indexes[indexes.Depth] + value).ToArray());
        public static TreeIndex operator -(TreeIndex indexes, int value) => indexes.IsNull ? default : new TreeIndex(indexes.m_indexes.SubArray(0, indexes.Depth).Append(indexes[indexes.Depth] - value).ToArray());
        public static TreeIndex operator *(TreeIndex start, TreeIndex end)
        {
            return new TreeIndex(EqualIndexes(start, end).ToArray());
            static IEnumerable<int> EqualIndexes(TreeIndex start, TreeIndex end)
            {
                for (int i = 0; i <= Min(start.Depth, end.Depth); i++)
                {
                    int current = start[i];
                    if (current == end[i]) yield return current;
                    else yield break;
                }
            }
        }
        public static TreeIndex operator <<(TreeIndex treeIndex, int depth) => new TreeIndex(depth > treeIndex.Depth ? default : treeIndex.m_indexes.SubArray(depth, treeIndex.m_indexes.Length - depth));
        public static TreeIndex operator >>(TreeIndex treeIndex, int depth) => new TreeIndex(depth > treeIndex.Depth ? default : treeIndex.m_indexes.SubArray(0, treeIndex.m_indexes.Length - depth));

        public static bool operator ==(TreeIndex left, TreeIndex right) => left.CompareTo(right) == 0;
        public static bool operator !=(TreeIndex left, TreeIndex right) => left.CompareTo(right) != 0;
        public static bool operator <(TreeIndex left, TreeIndex right) => left.CompareTo(right) < 0;
        public static bool operator >(TreeIndex left, TreeIndex right) => left.CompareTo(right) > 0;
        public static bool operator <=(TreeIndex left, TreeIndex right) => left.CompareTo(right) <= 0;
        public static bool operator >=(TreeIndex left, TreeIndex right) => left.CompareTo(right) >= 0;

        public static bool operator ==(TreeIndex left, TreeIndex? right) => left.IsNull && !right.HasValue || left == right.Value;
        public static bool operator !=(TreeIndex left, TreeIndex? right) => !(left == right);

        public static implicit operator TreeIndex(int index) => new TreeIndex(new[] { index });
    }

    //public interface IRangeTreeNode<T> : ITreeNode<T> { new IRangeTree<T> Children { get; } }
    public interface ITreeNode<T> { ITree<T> Children { get; } }

    public readonly struct EnumerableTreeNode<T>
    {
        private readonly IEnumerable<EnumerableTreeNode<T>> m_children;
        private readonly T m_value;

        public EnumerableTreeNode(T value) : this()
        {
            HasValue = true;
            m_value = value;
        }
        public EnumerableTreeNode(IEnumerable<EnumerableTreeNode<T>> children) : this() => m_children = children;

        public bool IsLeaf => HasValue && !(m_value is ITreeNode<T>);
        public bool HasValue { get; }

        public T Value => HasValue ? m_value : throw new InvalidOperationException("Cette instance n'a pas de valeur");
        public IEnumerable<EnumerableTreeNode<T>> Children => HasValue ? (m_value is ITreeNode<T> treeNode ? treeNode.Children : throw new InvalidOperationException("Cette instance est une feuille")) : m_children;
    }

    public interface ITree<T> : IEnumerable<EnumerableTreeNode<T>>
    {
        int Count { get; }
        bool IsReadOnly { get; }
        void Add(T item);
        void Clear();
        bool Contains(T item);
        bool Remove(T item);
        T this[TreeIndex index] { get; set; }
        TreeIndex IndexOf(T item);
        void Insert(TreeIndex index, T item);
        void RemoveAt(TreeIndex index);
        IList<T> Nodes { get; }
    }

    /*public interface IRangeTree<T> : ITree<T>
    {
        void AddRange(IEnumerable<T> collection);
        void InsertRange(TreeIndex index, IEnumerable<T> collection);
        void RemoveRange(Range<TreeIndex> range);
    }*/

    public interface INotifyTreeChanged<T> { event NotifyTreeChangedEventHandler<T> TreeChanged; }

    public class NotifyTreeChangedEventArgs<T> : EventArgs
    {
        public NotifyCollectionChangedAction Action { get; }

        public TreeIndex OldStartingIndex { get; }
        public IList<T> OldItems { get; }

        public TreeIndex NewStartingIndex { get; }
        public IList<T> NewItems { get; }
    }

    public delegate void NotifyTreeChangedEventHandler<T>(object sender, NotifyTreeChangedEventArgs<T> e);

    /*public class RangeTree<T> : Tree<T>//, IRangeTree<T>
    {
        protected virtual void InsertItemsRange(int index, IEnumerable<T> collection) => Nodes.InsertRange(index, collection);//collection.ForEach((i, item) => InsertItem(index + i, item));
        protected virtual void RemoveItemsRange(int index, int count) => Nodes.RemoveRange(index, count); //{ for (int i = index; i < count; i++) RemoveItem(i); }

        public void AddRange(IEnumerable<T> collection) => InsertItemsRange(Nodes.Count, collection);
        public void InsertRange(TreeIndex index, IEnumerable<T> collection)
        {
            if (index.Depth == 0) InsertItemsRange(index[0], collection);
            else if (Nodes[index[0]] is ITreeNode<T> treeNode)
            {
                if (treeNode.Children is Tree<T> tree) tree.InsertRange(index << 1, collection);
                else foreach (var node in treeNode.Children);
            }
            else throw new IndexOutOfRangeException("L'index spécifié pointe vers une profondeur qui n'existe pas dans cet arbre.");
        }

        public void RemoveRange(Range<TreeIndex> range) => throw new NotImplementedException();
    }*/

    public class ArrayTree<T> : ReadOnlyTreeBase<T>
    {
        public ArrayTree() : this(Array.Empty<T>()) { }
        public ArrayTree(int capacity) : this(new T[capacity]) { }
        public ArrayTree(IEnumerable<T> items) : this(items.ToArray()) { }
        public ArrayTree(params T[] items) => Nodes = items;

        public T[] Nodes { get; }
        protected override IList<T> Items => Nodes;
    }

    public class ObservableTree<T> : TreeBase<T>, INotifyTreeChanged<T>, INotifyCollectionChanged
    {
        public ObservableTree(ObservableRangeCollection<T> nodes)
        {
            Nodes = nodes;
        }

        public ObservableTree() : this(new ObservableRangeCollection<T>()) { }
        public ObservableTree(List<T> items) : this(new ObservableRangeCollection<T>(items)) { }
        public ObservableTree(IEnumerable<T> items) : this(new ObservableRangeCollection<T>(items)) { }
        public ObservableTree(params T[] items) : this(new ObservableRangeCollection<T>(items)) { }

        public ObservableRangeCollection<T> Nodes { get; }
        protected override IList<T> Items => Nodes;

        public virtual event NotifyTreeChangedEventHandler<T> TreeChanged;
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
    }

    public class Tree<T> : TreeBase<T>
    {
        public List<T> Nodes { get; }
        protected override IList<T> Items => Nodes;

        public Tree(List<T> nodes) => Nodes = nodes;
        public Tree() : this(new List<T>()) { }
        public Tree(int capacity) : this(new List<T>(capacity)) { }
        public Tree(IEnumerable<T> items) : this(new List<T>(items)) { }
        public Tree(params T[] items) : this(new List<T>(items)) { }
    }

    public abstract class TreeBase<T> : ReadOnlyTreeBase<T>, ITree<T>
    {
        public new T this[TreeIndex index]
        {
            get => base[index];
            set
            {
                if (index.Depth == 0) SetItem(index[0], value);
                else if (Items[index[0]] is ITreeNode<T> treeNode) treeNode.Children[index << 1] = value;
                else throw new IndexOutOfRangeException("L'index spécifié pointe vers une profondeur qui n'existe pas dans cet arbre.");
            }
        }

        protected virtual void ClearItems() => Items.Clear();
        protected virtual void InsertItem(int index, T item) => Items.Insert(index, item);
        protected virtual void RemoveItem(int index) => Items.RemoveAt(index);
        protected virtual void SetItem(int index, T item) => Items[index] = item;

        public void Add(T item) => InsertItem(Items.Count, item);
        public void Clear()
        {
            foreach (var node in Items.OfType<ITreeNode<T>>()) node.Children.Clear();
            ClearItems();
        }
        public void Insert(TreeIndex index, T item)
        {
            if (index.Depth == 0) InsertItem(index[0], item);
            else if (Items[index[0]] is ITreeNode<T> treeNode) treeNode.Children.Insert(index << 1, item);
            else throw new IndexOutOfRangeException("L'index spécifié pointe vers une profondeur qui n'existe pas dans cet arbre.");
        }
        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index.IsNull) return false;
            else
            {
                RemoveAt(index);
                return true;
            }
        }
        public void RemoveAt(TreeIndex index)
        {
            if (index.Depth == 0) RemoveItem(index[0]);
            else if (Items[index[0]] is ITreeNode<T> treeNode) treeNode.Children.RemoveAt(index << 1);
            else throw new IndexOutOfRangeException("L'index spécifié pointe vers une profondeur qui n'existe pas dans cet arbre.");
        }

        public override bool IsReadOnly => false;
    }

    public abstract class ReadOnlyTreeBase<T> : ITree<T>
    {
        protected abstract IList<T> Items { get; }
        IList<T> ITree<T>.Nodes => Items;

        public T this[TreeIndex index] => index.Depth == 0 ? Items[index[0]] : Items[index[0]] is ITreeNode<T> treeNode ? treeNode.Children[index << 1] : throw new IndexOutOfRangeException("L'index spécifié pointe vers une profondeur qui n'existe pas dans cet arbre.");
        T ITree<T>.this[TreeIndex index] { get => this[index]; set => throw new InvalidOperationException(); }

        public int Count => Items.Sum(node => node is ITreeNode<T> treeNode ? treeNode.Children.Count + 1 : 1);

        public virtual bool IsReadOnly => true;

        public bool Contains(T item) => Items.Any(node => node.Equals(item) || node is ITreeNode<T> treeNode && treeNode.Children.Contains(item));
        public TreeIndex IndexOf(T item) => Items.Select((node, i) => node is ITreeNode<T> treeNode ? i + treeNode.Children.IndexOf(item) : node.Equals(item) ? i : default).FirstOrDefault(index => !index.IsNull);

        void ITree<T>.Add(T item) => throw new InvalidOperationException();
        void ITree<T>.Clear() => throw new InvalidOperationException();
        bool ITree<T>.Remove(T item) => throw new InvalidOperationException();
        void ITree<T>.Insert(TreeIndex index, T item) => throw new InvalidOperationException();
        void ITree<T>.RemoveAt(TreeIndex index) => throw new InvalidOperationException();

        public IEnumerator<EnumerableTreeNode<T>> GetEnumerator() => Items.Select(node => new EnumerableTreeNode<T>(node)).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static partial class Extensions
    {
        public static IEnumerable<T> AllTreeLeafs<T>(this IEnumerable<EnumerableTreeNode<T>> tree)
        {
            foreach (var node in tree)
            {
                if (node.IsLeaf) yield return node.Value;
                else foreach (var leaf in node.Children.AllTreeLeafs()) yield return leaf;
            }
        }
        public static IEnumerable<T> AllTreeNodes<T>(this IEnumerable<EnumerableTreeNode<T>> tree)
        {
            foreach (var node in tree)
            {
                if (!node.HasValue) foreach (var child in node.Children.AllTreeNodes()) yield return child;
                else yield return node.Value;
            }
        }

        public static IEnumerable<EnumerableTreeNode<T>> SubTree<T>(this IEnumerable<EnumerableTreeNode<T>> tree, Range<TreeIndex> range, bool allowExcess)
        {
            var start = Ordinal<int>.Max(Ordinal<int>.Zero, range.Start.Convert(ti => ti.IsNull ? throw new ArgumentNullException("range.Start") : ti[0]));
            var end = range.End.Convert(ti => ti.IsNull ? throw new ArgumentNullException("range.End") : ti[0]);
            return tree.SubCollection((start, end), allowExcess).Select((node, i) =>
            start.Convert(st => st + i) == start && range.Start.Value.Depth > 0 ? SubTree(node, (range.Start.Value << 1, start.Convert(st => st + i) == end ? range.End.Value << 1 : PositiveInfinity), allowExcess) :
            start.Convert(st => st + i) == end && range.End.Value.Depth > 0 ? SubTree(node, (Zero, range.End.Value << 1), allowExcess) :
            node);
            static EnumerableTreeNode<T> SubTree(EnumerableTreeNode<T> treeNode, Range<TreeIndex> range, bool allowExcess) => treeNode.IsLeaf ? throw new InvalidOperationException("Cette instance est une feuille") : new EnumerableTreeNode<T>(treeNode.Children.SubTree(range, allowExcess));
        }

        public static Tree<T> ToTree<T, TNode>(this IEnumerable<EnumerableTreeNode<T>> tree, Func<Tree<T>, TNode> nodeCreator) where TNode : T, ITreeNode<T> => new Tree<T>(tree.Select(node => node.HasValue ? node.Value : nodeCreator(node.Children.ToTree(nodeCreator))));
        public static ArrayTree<T> ToArrayTree<T, TNode>(this IEnumerable<EnumerableTreeNode<T>> tree, Func<ArrayTree<T>, TNode> nodeCreator) where TNode : T, ITreeNode<T> => new ArrayTree<T>(tree.Select(node => node.HasValue ? node.Value : nodeCreator(node.Children.ToArrayTree(nodeCreator))));
    }
}
