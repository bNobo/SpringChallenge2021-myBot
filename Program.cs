﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Cell
{
    public int index;
    public int richess;
    public int[] neighbours;

    public Cell(int index, int richess, int[] neighbours)
    {
        this.index = index;
        this.richess = richess;
        this.neighbours = neighbours;
    }
}

class Tree
{
    public int cellIndex;
    public int size;
    public bool isMine;
    public bool isDormant;

    public Tree(int cellIndex, int size, bool isMine, bool isDormant)
    {
        this.cellIndex = cellIndex;
        this.size = size;
        this.isMine = isMine;
        this.isDormant = isDormant;
    }

    public override string ToString()
    {
        return $"{cellIndex}, {size}";
    }
}

class Action
{
    public const string WAIT = "WAIT";
    public const string SEED = "SEED";
    public const string GROW = "GROW";
    public const string COMPLETE = "COMPLETE";

    public static Action Parse(string action)
    {
        string[] parts = action.Split(" ");
        switch (parts[0])
        {
            case WAIT:
                return new Action(WAIT);
            case SEED:
                return new Action(SEED, int.Parse(parts[1]), int.Parse(parts[2]));
            case GROW:
            case COMPLETE:
            default:
                return new Action(parts[0], int.Parse(parts[1]));
        }
    }

    public string type;
    public int targetCellIdx;
    public int sourceCellIdx;
    private string _message;
    public void SetMessage(string value) => _message = " " + value;

    public Action(string type, int sourceCellIdx, int targetCellIdx)
    {
        this.type = type;
        this.targetCellIdx = targetCellIdx;
        this.sourceCellIdx = sourceCellIdx;
    }

    public Action(string type, int targetCellIdx)
        : this(type, 0, targetCellIdx)
    {
    }

    public Action(string type)
        : this(type, 0, 0)
    {
    }

    public override string ToString()
    {
        if (type == WAIT)
        {
            return WAIT + _message;
        }
        if (type == SEED)
        {
            return string.Format("{0} {1} {2}{3}", SEED, sourceCellIdx, targetCellIdx, _message);
        }
        return string.Format("{0} {1}{2}", type, targetCellIdx, _message);
    }
}

class ActionList : List<Action>
{
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (var action in this)
        {
            stringBuilder.AppendLine($"{action}");
        }

        return stringBuilder.ToString();
    }
}

class Game
{
    public int day;
    public int nutrients;
    public List<Cell> board;
    public List<Action> possibleActions;
    public List<Tree> trees;
    public int mySun, opponentSun;
    public int myScore, opponentScore;
    public bool opponentIsWaiting;
    public int sunDirection;

    public Game()
    {
        board = new List<Cell>();
        possibleActions = new ActionList();
        trees = new List<Tree>();
    }

    private Tree currentlyGrowingTree;
    private int phase = 1;
    private int centerTreeIndex = -1;
    private int numberOfMatureTrees = 0;
    private int tour = 0;
    private bool doSeed = false;

    private List<int[]> lignes = new List<int[]>
    {
        new [] { 25, 24, 23, 22 },
        new [] { 26, 11, 10, 9, 21 },
        new [] { 27, 12, 3, 2, 8, 20 },
        new [] { 28, 13, 4, 0, 1, 7, 19 },
        new [] { 29, 14, 5, 6, 18, 36 },
        new [] { 30, 15, 16, 17, 35 },
        new [] { 31, 32, 33, 34 }
    };

    private List<int[]> diagonales1 = new List<int[]>
    {
        new [] { 28, 29, 30, 31 },
        new [] { 27, 13, 14, 15, 32 },
        new [] { 26, 12, 4, 5, 16, 33 },
        new [] { 25, 11, 3, 0, 6, 17, 34 },
        new [] { 24, 10, 2, 1, 18, 35},
        new [] { 23, 9, 8, 7, 36 },
        new [] { 22, 21, 20, 19 }
    };

    private List<int[]> diagonales2 = new List<int[]>
    {
        new [] { 25, 26, 27, 28 },
        new [] { 24, 11, 12, 13, 29 },
        new [] { 23, 10, 3, 4, 14, 30 },
        new [] { 22, 9, 2, 0, 5, 15, 31 },
        new [] { 21, 8, 1, 6, 16, 32 },
        new [] { 20, 7, 18, 17, 33 },
        new [] { 19, 36, 35, 34 }
    };

    public Action GetNextAction()
    {
        Console.Error.WriteLine($"Phase {phase}");
        Console.Error.WriteLine($"Tour {tour}");
        Console.Error.WriteLine($"possible actions = \n{possibleActions}");

        Action nextAction;

        switch (phase)
        {
            case 1:
                nextAction = GetNextAction1();
                break;
            case 2:
                nextAction = GetNextAction2();
                break;
            case 3:
                nextAction = GetNextAction3();
                break;
            case 4:
                nextAction = GetNextAction4();
                break;
            default:
                nextAction = Action.Parse(Action.WAIT);
                break;
        }

        if (tour == 0)
        {
            nextAction.SetMessage("Que le meilleur gagne !");
        }

        tour++;

        return nextAction;
    }

    private Action GetNextAction1()
    {
        if (centerTreeIndex > -1)
        {
            var centerTree = trees.SingleOrDefault(_ => _.cellIndex == centerTreeIndex);

            if (centerTree == null)
            {
                // Il y a eu conflit, il faut retenter
                doSeed = true;
            }
            else
            {
                phase = 2;
                var res = GetNextAction2();
                res.SetMessage("Petit à petit l'oiseau fait son nid");
                return res;
            }
        }

        //var richestTrees = trees.OrderByDescending(_ => board[_.cellIndex].richess);
        IOrderedEnumerable<Action> centerActions = possibleActions
            .OrderBy(_ => _.targetCellIdx);

        Action action = Action.Parse(Action.WAIT);

        var growAction = centerActions
            .Where(_ => _.type == Action.GROW)
            .Where(_ => trees.Single(t => t.cellIndex == _.targetCellIdx).size == 0
            || trees.Single(t => t.cellIndex == _.targetCellIdx).size == 1)
            .OrderByDescending(_ => trees.Single(t => t.cellIndex == _.targetCellIdx).size)
            .FirstOrDefault();

        var seedAction = GetBestSeedAction(
            possibleActions
                .Where(_ => trees.SingleOrDefault(t => t.cellIndex == _.sourceCellIdx)?.size > 1)
                .OrderBy(_ => _.targetCellIdx)
            );

        /*if (tour == 8)
        {
            doSeed = true;
        }
        else
        {*/
        var numberOfSeeds = trees.Count(_ => _.isMine && _.size == 0);

        doSeed = numberOfSeeds == 0;
        //}

        if (doSeed)
        {
            action = seedAction ?? growAction ?? action;

            if (seedAction != null)
            {
                doSeed = false;
            }
        }
        else
        {
            if (growAction != null)
            {
                action = growAction;
                doSeed = true;
            }
        }

        if (action.type == Action.SEED)
        {
            int maxIndex = 6;

            //int richess = board.Single(_ => _.index == 0).richess;

            //if (richess > 0 && !trees.Any(t => t.cellIndex == 0))
            //{
            //    maxIndex = 0;
            //}

            if (seedAction.targetCellIdx <= maxIndex)
            {
                centerTreeIndex = seedAction.targetCellIdx;
                //phase = 2;
            }
        }

        return action;
    }
    private Action GetNextAction2()
    {
        var growBigTreeAction = possibleActions
            .Where(_ => _.type == Action.GROW
            && _.targetCellIdx != centerTreeIndex
            && trees.Any(t => t.cellIndex == _.targetCellIdx && t.size == 2)
            )
            .FirstOrDefault();

        var numberOfBigTrees = trees.Count(_ => _.isMine && _.size == 3);

        if (growBigTreeAction != null && numberOfBigTrees < 4)
        {
            if (growBigTreeAction.targetCellIdx < 7)
            {
                growBigTreeAction.SetMessage("Rome ne s'est pas construite en un jour !");
                phase = 3;
            }
            return growBigTreeAction;
        }

        var action = possibleActions
            .SingleOrDefault(_ => _.type == Action.GROW
            && _.targetCellIdx == centerTreeIndex);

        if (action != null)
        {
            var centerTree = trees.Single(_ => _.cellIndex == centerTreeIndex);

            if (centerTree.size == 2)
            {
                currentlyGrowingTree = null;
                numberOfMatureTrees = 1;
                action.SetMessage("Rome ne s'est pas construite en un jour !");
                phase = 3;
            }

            return action;
        }

        var numberOfSeeds = trees.Count(_ => _.isMine && _.size == 0);

        if (numberOfSeeds == 0)
        {
            var seedAction = GetBestSeedAction(possibleActions
                .Where(_ => _.type == Action.SEED)
                .OrderByDescending(_ => board[_.targetCellIdx].richess)
                .ThenBy(_ => _.targetCellIdx));

            if (seedAction != null)
            {
                return seedAction;
            }
        }

        action = possibleActions
            .Where(_ => _.type == Action.GROW)
            .FirstOrDefault();

        if (action != null)
        {
            return action;
        }

        return Action.Parse(Action.WAIT);
    }

    private Action GetNextAction3()
    {
        IOrderedEnumerable<Action> richestActions = possibleActions
            .OrderByDescending(_ => board[_.targetCellIdx].richess);

        if (day > 20)
        {
            phase = 4;
            var phase4Action = GetNextAction4();
            phase4Action.SetMessage("Qui sème le vent récolte la tempête !");
            return phase4Action;
        }

        Action action = null;

        var numberOfBigTrees = trees.Count(_ => _.isMine && _.size == 3);
        var numberOfSeeds = trees.Count(_ => _.isMine && _.size == 0);

        var completeActions = richestActions
            .Where(_ => _.type == Action.COMPLETE)
            .Where(_ => /*_.targetCellIdx > 6 && */numberOfBigTrees > 3);

        foreach (var completeAction in completeActions)
        {
            if (!WillProjectShadowOnEnemyTree(completeAction.targetCellIdx))
            {
                action = completeAction;
                break;
            }
        }

        if (action != null)
        {
            currentlyGrowingTree = null;

            numberOfSeeds++;

            return action;
        }

        Console.Error.WriteLine("Currently growing tree == {0}", currentlyGrowingTree);

        if (currentlyGrowingTree == null)
        {
            action = richestActions
                .FirstOrDefault(_ => _.type == Action.GROW);
        }
        else
        {
            action = possibleActions
                .Where(_ => _.type == Action.GROW)
                .FirstOrDefault(_ => _.targetCellIdx == currentlyGrowingTree.cellIndex);
        }

        if (action != null)
        {
            var targetTree = trees.Single(_ => _.cellIndex == action.targetCellIdx);
            currentlyGrowingTree = targetTree;

            if (targetTree.size == 0)
            {
                numberOfSeeds++;
            }
            else if (targetTree.size == 2)
            {
                numberOfMatureTrees++;
                currentlyGrowingTree = null;

            }

            return action;
        }

        if (numberOfSeeds == 0)
        {
            action = GetBestSeedAction(richestActions
                .Where(_ => _.type == Action.SEED)
                .Where(_ => trees.SingleOrDefault(t => t.cellIndex == _.sourceCellIdx)?.size > 1)
                .OrderByDescending(_ => board[_.targetCellIdx].richess)
                );

            if (action != null)
            {
                return action;
            }
        }

        var res = Action.Parse(Action.WAIT);

        return res;
    }

    // Retourne vrai si l'arbre présent sur index va projeter une ombre menaçante sur au moins un arbre ennemi au prochain tour
    private bool WillProjectShadowOnEnemyTree(int index)
    {
        bool res = false;

        int tomorrowSunDirection = sunDirection + 1;

        if (tomorrowSunDirection > 5) tomorrowSunDirection = 0;

        var tree = trees.Single(_ => _.cellIndex == index);

        int n = tree.size;

        for (int i = 0; i < n; i++)
        {
            var neighbourIndex = board[index].neighbours[tomorrowSunDirection];

            var neighbourTree = trees.SingleOrDefault(
                _ => !_.isMine
                && _.cellIndex == neighbourIndex
                && _.size <= tree.size);

            if (neighbourTree != null)
            {
                res = true;
                break;
            }

            index = neighbourIndex;

            if (index < 0) break;
        }

        return res;
    }

    // Retourne vrai si un arbre planté à l'index index va souffrir d'une ombre au prochain jour
    private bool WillBeVictimOfShadow(int index)
    {
        bool res = false;

        int tomorrowSunDirection = sunDirection + 1;

        if (tomorrowSunDirection > 5) tomorrowSunDirection = 0;

        int oppositeSunDirection = tomorrowSunDirection + 3;

        if (oppositeSunDirection > 5) oppositeSunDirection -= 6;

        int distance = 1;

        for (int i = 0; i < 3; i++)
        {
            var neighbourIndex = board[index].neighbours[oppositeSunDirection];

            var neighbourTree = trees.SingleOrDefault(
                _ => !_.isMine
                && _.cellIndex == neighbourIndex
                && _.size >= distance);

            if (neighbourTree != null)
            {
                res = true;
                break;
            }

            index = neighbourIndex;
            distance++;

            if (index < 0) break;
        }

        return res;
    }

    private Action GetBestSeedAction(IOrderedEnumerable<Action> seedActions)
    {
        foreach (var action in seedActions)
        {
            if (!WillSufferFromShadow(action.targetCellIdx))
            {
                return action;
            }
        }

        var targetCells = board.Where(c => seedActions.Select(a => a.targetCellIdx).Contains(c.index));

        foreach (var targetCell in targetCells)
        {
            var any = AnyNeighborTrees(targetCell);

            if (!any)
            {
                return seedActions.First(_ => _.targetCellIdx == targetCell.index);
            }
        }

        return seedActions
            .FirstOrDefault();
    }

    private bool AnyNeighborTrees(Cell cell)
    {
        for (int i = 0; i < 6; i++)
        {
            var neighborIndex = cell.neighbours[i];

            if (trees.Any(t => t.cellIndex == neighborIndex && t.isMine))
            {
                return true;
            }
        }

        return false;
    }

    private bool WillSufferFromShadow(int targetCellIdx)
    {
        var ligneIndex = lignes.FindIndex(_ => _.Contains(targetCellIdx));
        var diagonale1Index = diagonales1.FindIndex(_ => _.Contains(targetCellIdx));
        var diagonale2Index = diagonales2.FindIndex(_ => _.Contains(targetCellIdx));

        return ExistOtherTreeOn(lignes[ligneIndex], targetCellIdx)
            || ExistOtherTreeOn(diagonales1[diagonale1Index], targetCellIdx)
            || ExistOtherTreeOn(diagonales2[diagonale2Index], targetCellIdx);
    }

    private bool ExistOtherTreeOn(int[] indexes, int targetCellIdx)
    {
        List<int> list = indexes.ToList();
        var i2 = list.FindIndex(_ => _ == targetCellIdx);

        foreach (var index in indexes)
        {
            var tree = trees.SingleOrDefault(_ => _.cellIndex == index && _.isMine);

            if (index != targetCellIdx && tree != null)
            {
                var i1 = list.IndexOf(index);
                var dist = Math.Abs(i2 - i1);

                if (dist <= tree.size)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private Action GetNextAction4()
    {
        IOrderedEnumerable<Action> richestActions = possibleActions
            .OrderByDescending(_ => board[_.targetCellIdx].richess)
            .ThenBy(_ => _.targetCellIdx);

        var action = richestActions
            .FirstOrDefault(_ => _.type == Action.COMPLETE);

        var numberOfBigTrees = trees.Count(_ => _.size == 3 && _.isMine);

        if (action != null && numberOfBigTrees > 23 - day)
        {
            return action;
        }

        if (currentlyGrowingTree == null)
        {
            action = richestActions
                .FirstOrDefault(_ => _.type == Action.GROW);
        }
        else
        {
            action = possibleActions
                .Where(_ => _.type == Action.GROW)
                .FirstOrDefault(_ => _.targetCellIdx == currentlyGrowingTree.cellIndex);
        }

        if (action != null)
        {
            var targetTree = trees.Single(_ => _.cellIndex == action.targetCellIdx);

            currentlyGrowingTree = targetTree;

            return action;
        }

        return Action.Parse(Action.WAIT);
    }
}

class Player
{
    static void Main(string[] args)
    {
        string[] inputs;

        Game game = new Game();

        int numberOfCells = int.Parse(Console.ReadLine()); // 37
        for (int i = 0; i < numberOfCells; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int index = int.Parse(inputs[0]); // 0 is the center cell, the next cells spiral outwards
            int richness = int.Parse(inputs[1]); // 0 if the cell is unusable, 1-3 for usable cells
            int neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
            int neigh1 = int.Parse(inputs[3]);
            int neigh2 = int.Parse(inputs[4]);
            int neigh3 = int.Parse(inputs[5]);
            int neigh4 = int.Parse(inputs[6]);
            int neigh5 = int.Parse(inputs[7]);
            int[] neighs = new int[] { neigh0, neigh1, neigh2, neigh3, neigh4, neigh5 };
            Cell cell = new Cell(index, richness, neighs);
            game.board.Add(cell);
        }

        // game loop
        while (true)
        {
            game.day = int.Parse(Console.ReadLine()); // the game lasts 24 days: 0-23
            game.sunDirection = game.day % 6;
            game.nutrients = int.Parse(Console.ReadLine()); // the base score you gain from the next COMPLETE action
            inputs = Console.ReadLine().Split(' ');
            game.mySun = int.Parse(inputs[0]); // your sun points
            game.myScore = int.Parse(inputs[1]); // your current score
            inputs = Console.ReadLine().Split(' ');
            game.opponentSun = int.Parse(inputs[0]); // opponent's sun points
            game.opponentScore = int.Parse(inputs[1]); // opponent's score
            game.opponentIsWaiting = inputs[2] != "0"; // whether your opponent is asleep until the next day

            game.trees.Clear();
            int numberOfTrees = int.Parse(Console.ReadLine()); // the current amount of trees
            for (int i = 0; i < numberOfTrees; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int cellIndex = int.Parse(inputs[0]); // location of this tree
                int size = int.Parse(inputs[1]); // size of this tree: 0-3
                bool isMine = inputs[2] != "0"; // 1 if this is your tree
                bool isDormant = inputs[3] != "0"; // 1 if this tree is dormant
                Tree tree = new Tree(cellIndex, size, isMine, isDormant);
                game.trees.Add(tree);
            }

            game.possibleActions.Clear();
            int numberOfPossibleMoves = int.Parse(Console.ReadLine());
            for (int i = 0; i < numberOfPossibleMoves; i++)
            {
                string possibleMove = Console.ReadLine();
                game.possibleActions.Add(Action.Parse(possibleMove));
            }

            Action action = game.GetNextAction();

            Console.WriteLine(action);
        }
    }
}