using System;

namespace RangeTree;

public class RangeTree
{
    private INode _root;

    public void AddRange(int min, int max)
    {
        if (_root == null)
        {
            _root = new LeafNode(min, max);
            return;
        }

        INode inserted = InsertAt(_root, min, max);
        Validate(inserted, min, max, true);
        Validate(inserted, _root.Min, _root.Max, false);
        INode rebalanced = Rebalance(inserted);
        Validate(rebalanced, min, max, true);
        Validate(rebalanced, _root.Min, _root.Max, false);

        _root = rebalanced;

        void Validate(INode n, int a, int b, bool v)
        {
            if (!n.Contains(a))
                throw new InvalidOperationException();
            if (!n.Contains(b))
                throw new InvalidOperationException();
            if (v && !n.Contains((a+b)/2))
                throw new InvalidOperationException();
        }
    }

    private static INode InsertAt(INode node, int min, int max)
    {
        if (min <= node.Min && max >= node.Max)
        {
            // Old is inside of new, return a new leaf
            return new LeafNode(min, max);
        }
        
        // Non overlapping
        if (max < node.Min)
        {
            // Node is fully left
            return new TreeNode(new LeafNode(min, max), node);
        }

        if (min > node.Max)
        {
            // Node is fully right
            return new TreeNode(node, new LeafNode(min, max));
        }

        switch (node)
        {
            case LeafNode oLeaf:
                if (min >= node.Min && max <= node.Max)
                {
                    // New is inside old, just return old
                    return oLeaf;
                }

                // It's not a full overlap, so we need to expand it
                return new LeafNode(Math.Min(min, node.Min), Math.Max(max, node.Max));
            
            case TreeNode oTree:
                if (max < oTree.Right.Min)
                {
                    if (min > oTree.Left.Max)
                    {
                        // It could go in either, prefer to keep it balanced
                        if (oTree.Right.Depth < oTree.Left.Depth)
                        {
                            return oTree.WithRight(InsertAt(oTree.Right, min, max));
                        }
                    }

                    return oTree.WithLeft(InsertAt(oTree.Left, min, max));
                }

                if (min > oTree.Left.Max)
                    return oTree.WithRight(InsertAt(oTree.Right, min, max));
                if (min <= oTree.Left.Max && max >= oTree.Right.Min)
                {
                    // It spans the gap, we need to delete any node that
                    // Plan! Insert it on the left, then "steal" the left most node
                    // And insert that on the right

                    var insertedLeft = InsertAt(oTree.Left, min, max);
                    (INode newLeft, LeafNode extracted) = ExtractRightMostNode(insertedLeft);
                    var insertedRight = InsertAt(oTree.Right, extracted.Min, extracted.Max);
                    if (newLeft == null)
                        return insertedRight;
                    return new TreeNode(newLeft, insertedRight);
                }

                throw new InvalidOperationException();
            default:
                throw new NotSupportedException();
        }
    }

    private static (INode newTree, LeafNode rightNode) ExtractRightMostNode(INode node)
    {
        switch (node)
        {
            case LeafNode l:
                return (null, l);
            case TreeNode t:
                var (rl, rr) = ExtractRightMostNode(t.Right);
                if (rl == null)
                    return (t.Left, rr);
                return (new TreeNode(t.Left, rl), rr);
            default:
                throw new NotSupportedException();
        }
    }

    private static INode Rebalance(INode node)
    {
        return node switch
        {
            LeafNode leaf => leaf,
            TreeNode tree => Inner(tree),
            _ => throw new ArgumentOutOfRangeException(nameof(node), node, null),
        };

        static INode Inner(TreeNode n)
        {
            int lDepth = n.Left.Depth;
            int rDepth = n.Right.Depth;

            if (lDepth > rDepth + 1)
            {
                var lTree = (TreeNode)n.Left;
                return new TreeNode(lTree.Left, new TreeNode(lTree.Right, n.Right));
            }

            if (rDepth > lDepth + 1)
            {
                var rTree = (TreeNode)n.Right;
                return new TreeNode(new TreeNode(n.Left, rTree.Left), rTree.Right);
            }

            return n;
        }
    }

    public bool Contains(int value) => _root?.Contains(value) ?? false;

    public override string ToString() => _root?.ToString() ?? "[empty]";
    
    private interface INode
    {
        int Depth { get; }
        int Min { get; }
        int Max { get; }

        bool Contains(int value);
    }

    private class TreeNode : INode
    {
        public INode Left { get; }
        public INode Right { get; }

        public int Depth { get; }
        public int Min { get; }
        public int Max { get; }
    
        public bool Contains(int value)
        {
            if (value < Min || value > Max)
                return false;
            return Left.Contains(value) || Right.Contains(value);
        }

        public TreeNode(INode left, INode right)
        {
            if (right.Min < left.Max)
            {
                throw new ArgumentException("Overlapping ranges");
            }

            Min = left.Min;
            Max = right.Max;
            Depth = Math.Max(left.Depth, right.Depth) + 1;
            Left = left;
            Right = right;
        }

        public TreeNode WithLeft(INode left) => new(left, Right);
        public TreeNode WithRight(INode right) => new(Left, right);

        public override string ToString()
        {
            return $"[{Min} .. (children) .. {Max}]";
        }
    }

    private class LeafNode : INode
    {
        public LeafNode(int min, int max)
        {
            if (min > max)
                throw new ArgumentException("Min > Max");
            Min = min;
            Max = max;
        }

        public int Depth => 1;
        public int Min { get; }
        public int Max { get; }
    
        public bool Contains(int value) => value >= Min && value <= Max;
    
        public override string ToString()
        {
            return $"[{Min} .. {Max}]";
        }
    }
}