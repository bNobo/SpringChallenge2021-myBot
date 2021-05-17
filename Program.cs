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

    private int GetMaxIndex(float[] outputs)
    {
        int maxIndex = 0;
        float max = 0;

        for (int index = 0; index < outputs.Length; index++)
        {
            if (outputs[index] > max)
            {
                max = outputs[index];
                maxIndex = index;
            }
        }

        return maxIndex;
    }

    private Action InterpretOutputs(float[] outputs)
    {
        int maxIndex = GetMaxIndex(outputs);

        Action nextAction = SelectAction(maxIndex);

        return nextAction;
    }

    private Action SelectAction(int maxIndex)
    {
        Action nextAction = Action.Parse(Action.WAIT);

        var tree = trees.SingleOrDefault(_ => _.cellIndex == maxIndex);

        if (tree == null)
        {
            nextAction = possibleActions.FirstOrDefault(
                _ => _.type == Action.SEED
                && _.targetCellIdx == maxIndex
                ) ?? nextAction;
        }
        else
        {
            if (tree.size < 3)
            {
                nextAction = new Action(Action.GROW, maxIndex);
            }
            else
            {
                nextAction = new Action(Action.COMPLETE, maxIndex);
            }
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

    internal void LoadBase64(string base64bytes)
    {
        byte[] bytes = Convert.FromBase64String(base64bytes);

        Load(bytes);
    }

    internal void Load(byte[] bytes)
    {
        var memoryStream = new MemoryStream(bytes);
        System.IO.BinaryReader binaryReader = new BinaryReader(memoryStream);
        Load(binaryReader);
        memoryStream.Close();
        memoryStream.Dispose();
    }

    internal void Load(BinaryReader binaryReader)
    {
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
    }

    internal void Load(string fileName)
    {
        FileStream file = System.IO.File.OpenRead(fileName);

        System.IO.BinaryReader binaryReader = new BinaryReader(file);

        Load(binaryReader);

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
            NeuralNet neuralNet = new NeuralNet(191, 40, 37, 1);
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
    static string neuralNetBytes = "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";

    static void Main(string[] args)
    {
        string[] inputs;

        Game game = new Game();
        //game.brain = new NeuralNet(191, 100, 152, 1);

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
        else
        {
            var neuralNet = new NeuralNet();
            neuralNet.LoadBase64(neuralNetBytes);
            game.brain = neuralNet;
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