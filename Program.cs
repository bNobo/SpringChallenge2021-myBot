using System;
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
    public NeuralNet brain;

    public Game()
    {
        board = new List<Cell>();
        possibleActions = new ActionList();
        trees = new List<Tree>();
    }

    private int tour = 0;    

    private int[][] lignes = new int[7][]
    {
        new [] { 25, 24, 23, 22 },
        new [] { 26, 11, 10, 9, 21 },
        new [] { 27, 12, 3, 2, 8, 20 },
        new [] { 28, 13, 4, 0, 1, 7, 19 },
        new [] { 29, 14, 5, 6, 18, 36 },
        new [] { 30, 15, 16, 17, 35 },
        new [] { 31, 32, 33, 34 }
    };

    private int[][] diagonales1 = new int[7][]
    {
        new [] { 28, 29, 30, 31 },
        new [] { 27, 13, 14, 15, 32 },
        new [] { 26, 12, 4, 5, 16, 33 },
        new [] { 25, 11, 3, 0, 6, 17, 34 },
        new [] { 24, 10, 2, 1, 18, 35},
        new [] { 23, 9, 8, 7, 36 },
        new [] { 22, 21, 20, 19 }
    };

    private int[][] diagonales2 = new int[7][]
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
        Console.Error.WriteLine($"Tour {tour}");
        Console.Error.WriteLine($"possible actions = \n{possibleActions}");

        Action nextAction = Action.Parse(Action.WAIT);

        if (tour == 0)
        {
            nextAction.SetMessage("Que le meilleur gagne !");
        }

        tour++;

        var inputs = Look();
        nextAction = Think(inputs);

        return nextAction;
    }

    private Action Think(float[] inputs)
    {
        var outputs = brain.Output(inputs);
        Action nextAction = InterpretOutputs(outputs);

        return nextAction;
    }

    private int GetMaxIndex(IEnumerable<int> indexes, int offset, float[] outputs)
    {
        int maxIndex = indexes.First();
        float max = 0;

        foreach (var index in indexes)
        {
            var outputIndex = index + offset;

            if (outputs[outputIndex] > max)
            {
                max = outputs[outputIndex];
                maxIndex = index;
            }
        }

        return maxIndex;
    }

    private Action InterpretOutputs(float[] outputs)
    {
        int maxIndex = GetMaxIndex(new[] { 148, 149, 150, 151 }, 0, outputs);

        Action nextAction = SelectAction(outputs, maxIndex);

        return nextAction;
    }

    private Action SelectAction(float[] outputs, int maxIndex)
    {
        IEnumerable<int> targetIdx;
        Action nextAction = Action.Parse(Action.WAIT);

        switch (maxIndex)
        {
            case 148:
                if (!possibleActions.Any(_ => _.type == Action.GROW))
                {
                    break;
                    //return SelectAction(outputs, maxIndex + 1);
                }

                targetIdx = possibleActions.Where(_ => _.type == Action.GROW)
                    .Select(_ => _.targetCellIdx);

                maxIndex = GetMaxIndex(targetIdx, 0, outputs);

                nextAction = new Action(Action.GROW, maxIndex);

                break;
            case 149:
                if (!possibleActions.Any(_ => _.type == Action.SEED))
                {
                    break;
                    //return SelectAction(outputs, maxIndex + 1);
                }

                targetIdx = possibleActions.Where(_ => _.type == Action.SEED)
                    .Select(_ => _.targetCellIdx);

                var maxTargetIndex = GetMaxIndex(targetIdx, 74, outputs);

                var sourceIdx = possibleActions.Where(_ => _.type == Action.SEED)
                    .Where(_ => _.targetCellIdx == maxTargetIndex)
                    .Select(_ => _.sourceCellIdx);

                var maxSourceIndex = GetMaxIndex(sourceIdx, 37, outputs);

                nextAction = new Action(Action.SEED, maxSourceIndex, maxTargetIndex);

                break;
            case 150:
                if (!possibleActions.Any(_ => _.type == Action.COMPLETE))
                {
                    break;
                    //return SelectAction(outputs, maxIndex + 1);
                }

                targetIdx = possibleActions.Where(_ => _.type == Action.COMPLETE)
                    .Select(_ => _.targetCellIdx);

                maxIndex = GetMaxIndex(targetIdx, 111, outputs);

                nextAction = new Action(Action.COMPLETE, maxIndex);

                break;
            //case 151:
            //    if (!possibleActions.Any(_ => _.type == Action.WAIT))
            //    {
            //        break;
            //    }
            //    break;
            default:
                break;
        }

        return nextAction;
    }

    private float[] Look()
    {
        float[] inputs = new float[191];
        int index = 0;

        foreach (var cell in board)
        {
            var tree = trees.SingleOrDefault(t => t.cellIndex == cell.index);

            inputs[index] = cell.richess / 4.0f;

            if (tree != null)
            {
                inputs[index + 1] = tree.isMine ? 1 : 0;
                inputs[index + 2] = !tree.isDormant ? 1 : 0;
                inputs[index + 3] = (tree.size + 1) / 4.0f;
                inputs[index + 4] = 1;
            }
            else
            {
                inputs[index + 4] = 0.5f;
            }

            index += 5;
        }

        inputs[185] = day / 24.0f;
        inputs[186] = nutrients / 20.0f;
        inputs[187] = 1.0f / mySun;
        inputs[188] = 1.0f / opponentSun;
        inputs[189] = opponentIsWaiting ? 1 : 0.5f;
        inputs[190] = ((day % 6) + 1) / 6.0f;

        return inputs;
    }
}

public class Matrix
{
    public int rows, cols;
    public float[,] matrix;
    
    Random random = new Random();

    public Matrix()
    {

    }

    public Matrix(int r, int c)
    {
        rows = r;
        cols = c;
        matrix = new float[rows,cols];
    }

    public Matrix(float[,] m)
    {
        matrix = m;
        rows = matrix.GetLength(0);
        cols = matrix.GetLength(1);
    }

    public Matrix Dot(Matrix n)
    {
        Matrix result = new Matrix(rows, n.cols);

        if (cols == n.rows)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < n.cols; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < cols; k++)
                    {
                        sum += matrix[i,k] * n.matrix[k,j];
                    }
                    result.matrix[i,j] = sum;
                }
            }
        }
        return result;
    }

    public void Randomize()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // Nombre aléatoire entre -1 et 1
                matrix[i,j] = random.Next(-10000000, 10000001) / 10000000.0f;
            }
        }
    }

    public Matrix SingleColumnMatrixFromArray(float[] arr)
    {
        Matrix n = new Matrix(arr.Length, 1);
        for (int i = 0; i < arr.Length; i++)
        {
            n.matrix[i,0] = arr[i];
        }
        return n;
    }

    public float[] ToArray()
    {
        float[] arr = new float[rows * cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                arr[j + i * cols] = matrix[i,j];
            }
        }
        return arr;
    }

    public Matrix AddBias()
    {
        Matrix n = new Matrix(rows + 1, 1);
        for (int i = 0; i < rows; i++)
        {
            n.matrix[i,0] = matrix[i,0];
        }
        n.matrix[rows,0] = 1;
        return n;
    }

    public Matrix Activate()
    {
        Matrix n = new Matrix(rows, cols);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                n.matrix[i,j] = Relu(matrix[i,j]);
            }
        }
        return n;
    }

    float Relu(float x)
    {
        return Math.Max(0, x);
    }

    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // Nombre aléatoire entre 0 et 1
                float rand = random.Next(10000001) / 10000000.0f;
                if (rand < mutationRate)
                {
                    matrix[i,j] += RandomGaussian() / 5;

                    if (matrix[i,j] > 1)
                    {
                        matrix[i,j] = 1;
                    }
                    if (matrix[i,j] < -1)
                    {
                        matrix[i,j] = -1;
                    }
                }
            }
        }
    }

    float RandomGaussian(double mean = 0, double stdDev = 1)
    {
        double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        double randNormal =
                     mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        return (float)randNormal;
    }

    public Matrix Crossover(Matrix partner)
    {
        Matrix child = new Matrix(rows, cols);

        int randC = random.Next(cols);
        int randR = random.Next(rows);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if ((i < randR) || (i == randR && j <= randC))
                {
                    child.matrix[i,j] = matrix[i,j];
                }
                else
                {
                    child.matrix[i,j] = partner.matrix[i,j];
                }
            }
        }
        return child;
    }

    public Matrix Clone()
    {
        Matrix clone = new Matrix(rows, cols);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                clone.matrix[i,j] = matrix[i,j];
            }
        }
        return clone;
    }
}

public class NeuralNet
{
    public int iNodes, hNodes, oNodes, hLayers;
    public Matrix[] weights;

    public NeuralNet()
    {

    }

    public NeuralNet(int input, int hidden, int output, int hiddenLayers)
    {
        iNodes = input;
        hNodes = hidden;
        oNodes = output;
        hLayers = hiddenLayers;

        weights = new Matrix[hLayers + 1];
        weights[0] = new Matrix(hNodes, iNodes + 1);
        for (int i = 1; i < hLayers; i++)
        {
            weights[i] = new Matrix(hNodes, hNodes + 1);
        }
        weights[weights.Length - 1] = new Matrix(oNodes, hNodes + 1);

        foreach (Matrix w in weights)
        {
            w.Randomize();
        }
    }

    public void Mutate(float mr)
    {
        foreach (Matrix w in weights)
        {
            w.Mutate(mr);
        }
    }

    public float[] Output(float[] inputsArr)
    {
        Matrix inputs = weights[0].SingleColumnMatrixFromArray(inputsArr);

        Matrix curr_bias = inputs.AddBias();

        for (int i = 0; i < hLayers; i++)
        {
            Matrix hidden_ip = weights[i].Dot(curr_bias);
            Matrix hidden_op = hidden_ip.Activate();
            curr_bias = hidden_op.AddBias();
        }

        Matrix output_ip = weights[weights.Length - 1].Dot(curr_bias);
        Matrix output = output_ip.Activate();

        return output.ToArray();
    }

    public NeuralNet Crossover(NeuralNet partner)
    {
        NeuralNet child = new NeuralNet(iNodes, hNodes, oNodes, hLayers);
        for (int i = 0; i < weights.Length; i++)
        {
            child.weights[i] = weights[i].Crossover(partner.weights[i]);
        }
        return child;
    }

    public NeuralNet Clone()
    {
        NeuralNet clone = new NeuralNet(iNodes, hNodes, oNodes, hLayers);
        for (int i = 0; i < weights.Length; i++)
        {
            clone.weights[i] = weights[i].Clone();
        }

        return clone;
    }

    void Load(Matrix[] weight)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = weight[i];
        }
    }

    internal void Load(string fileName)
    {
        FileStream file = System.IO.File.OpenRead(fileName);

        System.IO.BinaryReader binaryReader = new BinaryReader(file);

        iNodes = binaryReader.ReadInt32();
        hNodes = binaryReader.ReadInt32();
        oNodes = binaryReader.ReadInt32();
        hLayers = binaryReader.ReadInt32();

        weights = new Matrix[hLayers + 1];
        weights[0] = new Matrix(hNodes, iNodes + 1);
        for (int i = 1; i < hLayers; i++)
        {
            weights[i] = new Matrix(hNodes, hNodes + 1);
        }
        weights[weights.Length - 1] = new Matrix(oNodes, hNodes + 1);

        for (int i = 0; i < weights.Length; i++)
        {
            for (int row = 0; row < weights[i].rows; row++)
            {
                for (int col = 0; col < weights[i].cols; col++)
                {
                    weights[i].matrix[row, col] = binaryReader.ReadSingle();
                }
            }
        }
        
        binaryReader.Close();
        binaryReader.Dispose();

        file.Close();
        file.Dispose();
    }

    internal void Save(string fileName)
    {
        FileStream file = System.IO.File.OpenWrite(fileName);
        
        System.IO.BinaryWriter binaryWriter = new BinaryWriter(file);
        binaryWriter.Write(iNodes);
        binaryWriter.Write(hNodes);
        binaryWriter.Write(oNodes);
        binaryWriter.Write(hLayers);

        foreach (var matrix in weights)
        {
            for (int row = 0; row < matrix.rows; row++)
            {
                for (int col = 0; col < matrix.cols; col++)
                {
                    binaryWriter.Write(matrix.matrix[row, col]);
                }
            }
        }

        binaryWriter.Close();
        file.Close();
        binaryWriter.Dispose();
        file.Dispose();
    }
}

class Population
{
    private NeuralNet[] brains;
    int population;
    int gen;
    private Random random;

    public Population(int population, int gen)
    {
        if (population <= 0)
        {
            population = 1;
        }

        if (population > 10000)
        {
            population = 10000;
        }

        this.population = population;
        this.gen = gen;
        random = new Random();
        Directory.CreateDirectory($"c:\\neuralNets\\gen{gen}");
    }

    internal void Generate()
    {
        brains = new NeuralNet[population];

        for (int i = 0; i < population; i++)
        {
            NeuralNet neuralNet = new NeuralNet(191, 100, 152, 1);
            brains[i] = neuralNet;
            neuralNet.Save($"c:\\neuralNets\\gen{gen}\\neuralNet{i:0000}.bin");
        }
    }

    internal void Evolve()
    {
        brains = new NeuralNet[population];

        // Charge la génération précédente en mémoire
        for (int i = 0; i < population; i++)
        {
            string fileName = $"c:\\neuralNets\\gen{gen - 1}\\neuralNet{i:0000}.bin";
            if (File.Exists(fileName))
            {
                var neuralNet = new NeuralNet();
                neuralNet.Load(fileName);
                brains[i] = neuralNet;
            }
        }

        var newGeneration = NaturalSelection();

        Save(newGeneration);
    }

    private void Save(NeuralNet[] newGeneration)
    {
        for (int i = 0; i < population; i++)
        {
            newGeneration[i].Save($"c:\\neuralNets\\gen{gen}\\neuralNet{i:0000}.bin");
        }
    }

    private NeuralNet[] NaturalSelection()
    {
        NeuralNet[] newGeneration = new NeuralNet[population];

        brains.Where(_ => _ != null)
            .Select(_ => _.Clone())
            .ToArray()
            .CopyTo(newGeneration, 0);

        for (int i = population / 10; i < population; i++)
        {
            NeuralNet child = SelectParent(newGeneration).Crossover(SelectParent(newGeneration));
            child.Mutate(0.05f);
            newGeneration[i] = child;
        }

        return newGeneration;
    }

    private NeuralNet SelectParent(NeuralNet[] newGeneration)
    {
        int index = random.Next(population / 10);

        return newGeneration[index];
    }
}

class Player
{
    static void Main(string[] args)
    {
        string[] inputs;

        Game game = new Game();
        game.brain = new NeuralNet(191, 100, 152, 1);

        if (args != null && args.Length > 0)
        {
            Console.Error.WriteLine("Arg[0] = {0}", args[0]);

            if (args[0] == "generate")
            {
                var generator = new Population(int.Parse(args[1]), 0);
                generator.Generate();

                return;
            }

            if (args[0] == "load")
            {
                var neuralNet = new NeuralNet();
                neuralNet.Load($"c:\\neuralNets\\gen{args[2]}\\neuralNet{int.Parse(args[1]):0000}.bin");
                game.brain = neuralNet;
            }

            if (args[0] == "evolution")
            {
                int gen = int.Parse(args[1]);
                int population = int.Parse(args[2]);

                var generator = new Population(population, gen);

                if (gen == 0)
                {
                    generator.Generate();
                }
                else
                {
                    generator.Evolve();
                }

                Console.WriteLine("OK");

                return;
            }
        }

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