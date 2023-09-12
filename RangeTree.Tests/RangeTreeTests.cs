namespace RangeTree.Tests;

public class RangeTreeTests
{
    [Test]
    public void NoNodeTest()
    {
        RangeTree<int> t = new ();
        t.Contains(5).Should().BeFalse();
    }

    [Test]
    public void SingleRangeContains()
    {
        RangeTree<int> t = new();
        t.AddRange(3, 7);
        t.Contains(3).Should().BeTrue();
        t.Contains(5).Should().BeTrue();
        t.Contains(7).Should().BeTrue();
    }
    
    [Test]
    public void SingleRangeNoContains()
    {
        RangeTree<int> t = new();
        t.AddRange(3, 7);
        t.Contains(1).Should().BeFalse();
        t.Contains(10).Should().BeFalse();
    }

    [Test]
    public void DoubleRangeContains()
    {
        RangeTree<int> t = new();
        t.AddRange(3, 7);
        t.AddRange(13, 17);
        t.Contains(3).Should().BeTrue();
        t.Contains(5).Should().BeTrue();
        t.Contains(7).Should().BeTrue();
        t.Contains(13).Should().BeTrue();
        t.Contains(15).Should().BeTrue();
        t.Contains(17).Should().BeTrue();
    }
    
    [Test]
    public void DoubleRangeNoContains()
    {
        RangeTree<int> t = new();
        t.AddRange(3, 7);
        t.AddRange(13, 17);
        t.Contains(1).Should().BeFalse();
        t.Contains(10).Should().BeFalse();
        t.Contains(11).Should().BeFalse();
        t.Contains(20).Should().BeFalse();
    }

    [Test]
    public void TestRebalanceRight()
    {
        RangeTree<int> t = new();
        for (int i = 1; i < 100; i += 2)
        {
            t.AddRange(i, i);
        }

        for (int i = 1; i < 100; i += 2)
        {
            t.Contains(i).Should().BeTrue();
            t.Contains(i+1).Should().BeFalse();
        }
    }

    [Test]
    public void TestRebalanceLeft()
    {
        RangeTree<int> t = new();
        for (int i = 99; i >= 1; i -= 2)
        {
            t.AddRange(i, i);
        }

        for (int i = 1; i < 100; i += 2)
        {
            t.Contains(i).Should().BeTrue();
            t.Contains(i+1).Should().BeFalse();
        }
    }

    [Test]
    public void OverlapAll()
    {
        RangeTree<int> t = new();
        t.AddRange(5, 5);
        t.AddRange(3, 7);
        t.Contains(5).Should().BeTrue();
    }

    [Test]
    public void OverlapInside()
    {
        RangeTree<int> t = new();
        t.AddRange(3, 7);
        t.AddRange(5, 5);
        t.Contains(5).Should().BeTrue();
    }
    
    [Test]
    public void OverlapLeft()
    {
        RangeTree<int> t = new();
        t.AddRange(5, 7);
        t.AddRange(3, 6);
        t.Contains(5).Should().BeTrue();
    }
    
    [Test]
    public void OverlapRight()
    {
        RangeTree<int> t = new();
        t.AddRange(3, 6);
        t.AddRange(5, 7);
        t.Contains(5).Should().BeTrue();
    }
    
    [Test]
    public void OverlapJoining()
    {
        RangeTree<int> t = new();
        t.AddRange(2, 4);
        t.AddRange(6, 8);
        t.AddRange(3, 7);
        t.Contains(5).Should().BeTrue();
    }
    
    [Test]
    public void MinJoin()
    {
        RangeTree<int> t = new();
        t.AddRange(1, 3);
        t.AddRange(20, 22);
        
        t.AddRange(5, 7);
        t.AddRange(16, 18);
        t.Contains(5).Should().BeTrue();
        
        t.AddRange(6, 17);
        t.Contains(5).Should().BeTrue();
        t.Contains(4).Should().BeFalse();
        t.Contains(19).Should().BeFalse();
    }

    [Test]
    public void TestRandom()
    {
        for (int i = 0; i < 10000; i++)
        {
            TestRanges(Enumerable.Repeat(0, 100).Select(_ =>
                {
                    int a = Random.Shared.Next(-1_000_000, 1_000_000);
                    int b = Random.Shared.Next(-1_000_000, 1_000_000);
                    if (a > b)
                        (b, a) = (a, b);
                    return (a, b);
                }
            ).ToArray());
        }
    }

    private void TestRanges(params (int a, int b)[] ranges)
    {
        RangeTree<int> t = new();
        foreach ((int a, int b) in ranges)
        {
            t.AddRange(a, b);
        }
        
        foreach ((int a, int b) in ranges)
        {
            t.Contains(a).Should().BeTrue();
            t.Contains(b).Should().BeTrue();
            t.Contains((b + a) / 2).Should().BeTrue();
        }
    }
}