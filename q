[1mdiff --git a/Program.cs b/Program.cs[m
[1mindex 7cdcb4f..e75ca5f 100644[m
[1m--- a/Program.cs[m
[1m+++ b/Program.cs[m
[36m@@ -125,6 +125,7 @@[m [mclass Game[m
     public int mySun, opponentSun;[m
     public int myScore, opponentScore;[m
     public bool opponentIsWaiting;[m
[32m+[m[32m    public NeuralNet brain;[m[41m[m
 [m
     public Game()[m
     {[m
[36m@@ -133,14 +134,9 @@[m [mclass Game[m
         trees = new List<Tree>();[m
     }[m
 [m
[31m-    private Tree currentlyGrowingTree;[m
[31m-    private int phase = 1;[m
[31m-    private int centerTreeIndex = -1;[m
[31m-    private int numberOfMatureTrees = 0;[m
[31m-    private int tour = 0;[m
[31m-    private bool doSeed = false;[m
[32m+[m[32m    private int tour = 0;[m[41m    [m
 [m
[31m-    private List<int[]> lignes = new List<int[]>[m
[32m+[m[32m    private int[][] lignes = new int[7][][m[41m[m
     {[m
         new [] { 25, 24, 23, 22 },[m
         new [] { 26, 11, 10, 9, 21 },[m
[36m@@ -151,7 +147,7 @@[m [mclass Game[m
         new [] { 31, 32, 33, 34 }[m
     };[m
 [m
[31m-    private List<int[]> diagonales1 = new List<int[]>[m
[32m+[m[32m    private int[][] diagonales1 = new int[7][][m[41m[m
     {[m
         new [] { 28, 29, 30, 31 },[m
         new [] { 27, 13, 14, 15, 32 },[m
[36m@@ -162,7 +158,7 @@[m [mclass Game[m
         new [] { 22, 21, 20, 19 }[m
     };[m
 [m
[31m-    private List<int[]> diagonales2 = new List<int[]>[m
[32m+[m[32m    private int[][] diagonales2 = new int[7][][m[41m[m
     {[m
         new [] { 25, 26, 27, 28 },[m
         new [] { 24, 11, 12, 13, 29 },[m
[36m@@ -175,7 +171,6 @@[m [mclass Game[m
 [m
     public Action GetNextAction()[m
     {[m
[31m-        Console.Error.WriteLine($"Phase {phase}");[m
         Console.Error.WriteLine($"Tour {tour}");[m
         Console.Error.WriteLine($"possible actions = \n{possibleActions}");[m
 [m
[36m@@ -187,8 +182,578 @@[m [mclass Game[m
         }[m
 [m
         tour++;[m
[32m+[m[41m[m
[32m+[m[32m        var inputs = Look();[m[41m[m
[32m+[m[32m        nextAction = Think(inputs);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return nextAction;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    private Action Think(float[] inputs)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        var outputs = brain.Output(inputs);[m[41m[m
[32m+[m[32m        Action nextAction = InterpretOutputs(outputs);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return nextAction;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    private int GetMaxIndex(IEnumerable<int> indexes, int offset, float[] outputs)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        int maxIndex = indexes.First();[m[41m[m
[32m+[m[32m        float max = 0;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        foreach (var index in indexes)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            var outputIndex = index + offset;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m            if (outputs[outputIndex] > max)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                max = outputs[outputIndex];[m[41m[m
[32m+[m[32m                maxIndex = index;[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return maxIndex;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    private Action InterpretOutputs(float[] outputs)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        int maxIndex = GetMaxIndex(new[] { 148, 149, 150, 151 }, 0, outputs);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        Action nextAction = SelectAction(outputs, maxIndex);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return nextAction;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    private Action SelectAction(float[] outputs, int maxIndex)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        IEnumerable<int> targetIdx;[m[41m[m
[32m+[m[32m        Action nextAction = Action.Parse(Action.WAIT);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        switch (maxIndex)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            case 148:[m[41m[m
[32m+[m[32m                if (!possibleActions.Any(_ => _.type == Action.GROW))[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    break;[m[41m[m
[32m+[m[32m                    //return SelectAction(outputs, maxIndex + 1);[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                targetIdx = possibleActions.Where(_ => _.type == Action.GROW)[m[41m[m
[32m+[m[32m                    .Select(_ => _.targetCellIdx);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                maxIndex = GetMaxIndex(targetIdx, 0, outputs);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                nextAction = new Action(Action.GROW, maxIndex);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                break;[m[41m[m
[32m+[m[32m            case 149:[m[41m[m
[32m+[m[32m                if (!possibleActions.Any(_ => _.type == Action.SEED))[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    break;[m[41m[m
[32m+[m[32m                    //return SelectAction(outputs, maxIndex + 1);[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                targetIdx = possibleActions.Where(_ => _.type == Action.SEED)[m[41m[m
[32m+[m[32m                    .Select(_ => _.targetCellIdx);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                var maxTargetIndex = GetMaxIndex(targetIdx, 74, outputs);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                var sourceIdx = possibleActions.Where(_ => _.type == Action.SEED)[m[41m[m
[32m+[m[32m                    .Where(_ => _.targetCellIdx == maxTargetIndex)[m[41m[m
[32m+[m[32m                    .Select(_ => _.sourceCellIdx);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                var maxSourceIndex = GetMaxIndex(sourceIdx, 37, outputs);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                nextAction = new Action(Action.SEED, maxSourceIndex, maxTargetIndex);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                break;[m[41m[m
[32m+[m[32m            case 150:[m[41m[m
[32m+[m[32m                if (!possibleActions.Any(_ => _.type == Action.COMPLETE))[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    break;[m[41m[m
[32m+[m[32m                    //return SelectAction(outputs, maxIndex + 1);[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                targetIdx = possibleActions.Where(_ => _.type == Action.COMPLETE)[m[41m[m
[32m+[m[32m                    .Select(_ => _.targetCellIdx);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                maxIndex = GetMaxIndex(targetIdx, 111, outputs);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                nextAction = new Action(Action.COMPLETE, maxIndex);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                break;[m[41m[m
[32m+[m[32m            //case 151:[m[41m[m
[32m+[m[32m            //    if (!possibleActions.Any(_ => _.type == Action.WAIT))[m[41m[m
[32m+[m[32m            //    {[m[41m[m
[32m+[m[32m            //        break;[m[41m[m
[32m+[m[32m            //    }[m[41m[m
[32m+[m[32m            //    break;[m[41m[m
[32m+[m[32m            default:[m[41m[m
[32m+[m[32m                break;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return nextAction;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    private float[] Look()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        float[] inputs = new float[191];[m[41m[m
[32m+[m[32m        int index = 0;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        foreach (var cell in board)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            var tree = trees.SingleOrDefault(t => t.cellIndex == cell.index);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m            inputs[index] = cell.richess / 4.0f;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m            if (tree != null)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                inputs[index + 1] = tree.isMine ? 1 : 0;[m[41m[m
[32m+[m[32m                inputs[index + 2] = !tree.isDormant ? 1 : 0;[m[41m[m
[32m+[m[32m                inputs[index + 3] = (tree.size + 1) / 4.0f;[m[41m[m
[32m+[m[32m                inputs[index + 4] = 1;[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            else[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                inputs[index + 4] = 0.5f;[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m            index += 5;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        inputs[185] = day / 24.0f;[m[41m[m
[32m+[m[32m        inputs[186] = nutrients / 20.0f;[m[41m[m
[32m+[m[32m        inputs[187] = 1.0f / mySun;[m[41m[m
[32m+[m[32m        inputs[188] = 1.0f / opponentSun;[m[41m[m
[32m+[m[32m        inputs[189] = opponentIsWaiting ? 1 : 0.5f;[m[41m[m
[32m+[m[32m        inputs[190] = ((day % 6) + 1) / 6.0f;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return inputs;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[32m}[m[41m[m
[32m+[m[41m[m
[32m+[m[32mpublic class Matrix[m[41m[m
[32m+[m[32m{[m[41m[m
[32m+[m[32m    public int rows, cols;[m[41m[m
[32m+[m[32m    public float[,] matrix;[m[41m[m
[32m+[m[41m    [m
[32m+[m[32m    Random random = new Random();[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Matrix()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Matrix(int r, int c)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        rows = r;[m[41m[m
[32m+[m[32m        cols = c;[m[41m[m
[32m+[m[32m        matrix = new float[rows,cols];[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Matrix(float[,] m)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        matrix = m;[m[41m[m
[32m+[m[32m        rows = matrix.GetLength(0);[m[41m[m
[32m+[m[32m        cols = matrix.GetLength(1);[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Matrix Dot(Matrix n)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        Matrix result = new Matrix(rows, n.cols);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        if (cols == n.rows)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            for (int i = 0; i < rows; i++)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                for (int j = 0; j < n.cols; j++)[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    float sum = 0;[m[41m[m
[32m+[m[32m                    for (int k = 0; k < cols; k++)[m[41m[m
[32m+[m[32m                    {[m[41m[m
[32m+[m[32m                        sum += matrix[i,k] * n.matrix[k,j];[m[41m[m
[32m+[m[32m                    }[m[41m[m
[32m+[m[32m                    result.matrix[i,j] = sum;[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        return result;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public void Randomize()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        for (int i = 0; i < rows; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            for (int j = 0; j < cols; j++)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                // Nombre aléatoire entre -1 et 1[m[41m[m
[32m+[m[32m                matrix[i,j] = random.Next(-10000000, 10000001) / 10000000.0f;[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Matrix SingleColumnMatrixFromArray(float[] arr)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        Matrix n = new Matrix(arr.Length, 1);[m[41m[m
[32m+[m[32m        for (int i = 0; i < arr.Length; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            n.matrix[i,0] = arr[i];[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        return n;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public float[] ToArray()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        float[] arr = new float[rows * cols];[m[41m[m
[32m+[m[32m        for (int i = 0; i < rows; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            for (int j = 0; j < cols; j++)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                arr[j + i * cols] = matrix[i,j];[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        return arr;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Matrix AddBias()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        Matrix n = new Matrix(rows + 1, 1);[m[41m[m
[32m+[m[32m        for (int i = 0; i < rows; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            n.matrix[i,0] = matrix[i,0];[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        n.matrix[rows,0] = 1;[m[41m[m
[32m+[m[32m        return n;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Matrix Activate()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        Matrix n = new Matrix(rows, cols);[m[41m[m
[32m+[m[32m        for (int i = 0; i < rows; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            for (int j = 0; j < cols; j++)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                n.matrix[i,j] = Relu(matrix[i,j]);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        return n;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    float Relu(float x)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        return Math.Max(0, x);[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public void Mutate(float mutationRate)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        for (int i = 0; i < rows; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            for (int j = 0; j < cols; j++)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                // Nombre aléatoire entre 0 et 1[m[41m[m
[32m+[m[32m                float rand = random.Next(10000001) / 10000000.0f;[m[41m[m
[32m+[m[32m                if (rand < mutationRate)[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    matrix[i,j] += RandomGaussian() / 5;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                    if (matrix[i,j] > 1)[m[41m[m
[32m+[m[32m                    {[m[41m[m
[32m+[m[32m                        matrix[i,j] = 1;[m[41m[m
[32m+[m[32m                    }[m[41m[m
[32m+[m[32m                    if (matrix[i,j] < -1)[m[41m[m
[32m+[m[32m                    {[m[41m[m
[32m+[m[32m                        matrix[i,j] = -1;[m[41m[m
[32m+[m[32m                    }[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    float RandomGaussian(double mean = 0, double stdDev = 1)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles[m[41m[m
[32m+[m[32m        double u2 = 1.0 - random.NextDouble();[m[41m[m
[32m+[m[32m        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *[m[41m[m
[32m+[m[32m                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)[m[41m[m
[32m+[m[32m        double randNormal =[m[41m[m
[32m+[m[32m                     mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)[m[41m[m
[32m+[m[32m        return (float)randNormal;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Matrix Crossover(Matrix partner)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        Matrix child = new Matrix(rows, cols);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        int randC = random.Next(cols);[m[41m[m
[32m+[m[32m        int randR = random.Next(rows);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        for (int i = 0; i < rows; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            for (int j = 0; j < cols; j++)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                if ((i < randR) || (i == randR && j <= randC))[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    child.matrix[i,j] = matrix[i,j];[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[32m                else[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    child.matrix[i,j] = partner.matrix[i,j];[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        return child;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Matrix Clone()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        Matrix clone = new Matrix(rows, cols);[m[41m[m
[32m+[m[32m        for (int i = 0; i < rows; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            for (int j = 0; j < cols; j++)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                clone.matrix[i,j] = matrix[i,j];[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        return clone;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[32m}[m[41m[m
[32m+[m[41m[m
[32m+[m[32mpublic class NeuralNet[m[41m[m
[32m+[m[32m{[m[41m[m
[32m+[m[32m    public int iNodes, hNodes, oNodes, hLayers;[m[41m[m
[32m+[m[32m    public Matrix[] weights;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public NeuralNet()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public NeuralNet(int input, int hidden, int output, int hiddenLayers)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        iNodes = input;[m[41m[m
[32m+[m[32m        hNodes = hidden;[m[41m[m
[32m+[m[32m        oNodes = output;[m[41m[m
[32m+[m[32m        hLayers = hiddenLayers;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        weights = new Matrix[hLayers + 1];[m[41m[m
[32m+[m[32m        weights[0] = new Matrix(hNodes, iNodes + 1);[m[41m[m
[32m+[m[32m        for (int i = 1; i < hLayers; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            weights[i] = new Matrix(hNodes, hNodes + 1);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        weights[weights.Length - 1] = new Matrix(oNodes, hNodes + 1);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        foreach (Matrix w in weights)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            w.Randomize();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public void Mutate(float mr)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        foreach (Matrix w in weights)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            w.Mutate(mr);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public float[] Output(float[] inputsArr)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        Matrix inputs = weights[0].SingleColumnMatrixFromArray(inputsArr);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        Matrix curr_bias = inputs.AddBias();[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        for (int i = 0; i < hLayers; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            Matrix hidden_ip = weights[i].Dot(curr_bias);[m[41m[m
[32m+[m[32m            Matrix hidden_op = hidden_ip.Activate();[m[41m[m
[32m+[m[32m            curr_bias = hidden_op.AddBias();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        Matrix output_ip = weights[weights.Length - 1].Dot(curr_bias);[m[41m[m
[32m+[m[32m        Matrix output = output_ip.Activate();[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return output.ToArray();[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public NeuralNet Crossover(NeuralNet partner)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        NeuralNet child = new NeuralNet(iNodes, hNodes, oNodes, hLayers);[m[41m[m
[32m+[m[32m        for (int i = 0; i < weights.Length; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            child.weights[i] = weights[i].Crossover(partner.weights[i]);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        return child;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public NeuralNet Clone()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        NeuralNet clone = new NeuralNet(iNodes, hNodes, oNodes, hLayers);[m[41m[m
[32m+[m[32m        for (int i = 0; i < weights.Length; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            clone.weights[i] = weights[i].Clone();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return clone;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    void Load(Matrix[] weight)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        for (int i = 0; i < weights.Length; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            weights[i] = weight[i];[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    internal void Load(string fileName)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        FileStream file = System.IO.File.OpenRead(fileName);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        System.IO.BinaryReader binaryReader = new BinaryReader(file);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        iNodes = binaryReader.ReadInt32();[m[41m[m
[32m+[m[32m        hNodes = binaryReader.ReadInt32();[m[41m[m
[32m+[m[32m        oNodes = binaryReader.ReadInt32();[m[41m[m
[32m+[m[32m        hLayers = binaryReader.ReadInt32();[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        weights = new Matrix[hLayers + 1];[m[41m[m
[32m+[m[32m        weights[0] = new Matrix(hNodes, iNodes + 1);[m[41m[m
[32m+[m[32m        for (int i = 1; i < hLayers; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            weights[i] = new Matrix(hNodes, hNodes + 1);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        weights[weights.Length - 1] = new Matrix(oNodes, hNodes + 1);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        for (int i = 0; i < weights.Length; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            for (int row = 0; row < weights[i].rows; row++)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                for (int col = 0; col < weights[i].cols; col++)[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    weights[i].matrix[row, col] = binaryReader.ReadSingle();[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
         [m
[31m-        return nextAction;        [m
[32m+[m[32m        binaryReader.Close();[m[41m[m
[32m+[m[32m        binaryReader.Dispose();[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        file.Close();[m[41m[m
[32m+[m[32m        file.Dispose();[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    internal void Save(string fileName)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        FileStream file = System.IO.File.OpenWrite(fileName);[m[41m[m
[32m+[m[41m        [m
[32m+[m[32m        System.IO.BinaryWriter binaryWriter = new BinaryWriter(file);[m[41m[m
[32m+[m[32m        binaryWriter.Write(iNodes);[m[41m[m
[32m+[m[32m        binaryWriter.Write(hNodes);[m[41m[m
[32m+[m[32m        binaryWriter.Write(oNodes);[m[41m[m
[32m+[m[32m        binaryWriter.Write(hLayers);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        foreach (var matrix in weights)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            for (int row = 0; row < matrix.rows; row++)[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                for (int col = 0; col < matrix.cols; col++)[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    binaryWriter.Write(matrix.matrix[row, col]);[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        binaryWriter.Close();[m[41m[m
[32m+[m[32m        file.Close();[m[41m[m
[32m+[m[32m        binaryWriter.Dispose();[m[41m[m
[32m+[m[32m        file.Dispose();[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[32m}[m[41m[m
[32m+[m[41m[m
[32m+[m[32mclass Population[m[41m[m
[32m+[m[32m{[m[41m[m
[32m+[m[32m    private NeuralNet[] brains;[m[41m[m
[32m+[m[32m    int population;[m[41m[m
[32m+[m[32m    int gen;[m[41m[m
[32m+[m[32m    private Random random;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    public Population(int population, int gen)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        if (population <= 0)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            population = 1;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        if (population > 10000)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            population = 10000;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        this.population = population;[m[41m[m
[32m+[m[32m        this.gen = gen;[m[41m[m
[32m+[m[32m        random = new Random();[m[41m[m
[32m+[m[32m        Directory.CreateDirectory($"c:\\neuralNets\\gen{gen}");[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    internal void Generate()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        brains = new NeuralNet[population];[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        for (int i = 0; i < population; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            NeuralNet neuralNet = new NeuralNet(191, 100, 152, 1);[m[41m[m
[32m+[m[32m            brains[i] = neuralNet;[m[41m[m
[32m+[m[32m            neuralNet.Save($"c:\\neuralNets\\gen{gen}\\neuralNet{i:0000}.bin");[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    internal void Evolve(int maxIndex)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        brains = new NeuralNet[population];[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        // Charge la génération précédente en mémoire[m[41m[m
[32m+[m[32m        for (int i = 0; i < population; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            var neuralNet = new NeuralNet();[m[41m[m
[32m+[m[32m            neuralNet.Load($"c:\\neuralNets\\gen{gen - 1}\\neuralNet{i:0000}.bin");[m[41m[m
[32m+[m[32m            brains[i] = neuralNet;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        var newGeneration = NaturalSelection(maxIndex);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        Save(newGeneration);[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    private void Save(NeuralNet[] newGeneration)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        for (int i = 0; i < population; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            newGeneration[i].Save($"c:\\neuralNets\\gen{gen}\\neuralNet{i:0000}.bin");[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    private NeuralNet[] NaturalSelection(int maxIndex)[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        NeuralNet[] newGeneration = new NeuralNet[population];[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        newGeneration[0] = brains[maxIndex].Clone();[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        for (int i = 1; i < brains.Length; i++)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            NeuralNet child = SelectParent().Crossover(SelectParent());[m[41m[m
[32m+[m[32m            child.Mutate(0.05f);[m[41m[m
[32m+[m[32m            newGeneration[i] = child;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return newGeneration;[m[41m[m
[32m+[m[32m    }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m    private NeuralNet SelectParent()[m[41m[m
[32m+[m[32m    {[m[41m[m
[32m+[m[32m        int index = random.Next(population);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        return brains[index];[m[41m[m
     }[m
 }[m
 [m
[36m@@ -199,6 +764,49 @@[m [mclass Player[m
         string[] inputs;[m
 [m
         Game game = new Game();[m
[32m+[m[32m        game.brain = new NeuralNet(191, 100, 152, 1);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        if (args != null && args.Length > 0)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            Console.Error.WriteLine("Arg[0] = {0}", args[0]);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m            if (args[0] == "generate")[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                var generator = new Population(int.Parse(args[1]), 0);[m[41m[m
[32m+[m[32m                generator.Generate();[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                return;[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m            if (args[0] == "load")[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                var neuralNet = new NeuralNet();[m[41m[m
[32m+[m[32m                neuralNet.Load($"c:\\neuralNets\\gen{args[2]}\\neuralNet{int.Parse(args[1]):0000}.bin");[m[41m[m
[32m+[m[32m                game.brain = neuralNet;[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m            if (args[0] == "evolution")[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                int gen = int.Parse(args[1]);[m[41m[m
[32m+[m[32m                int maxIndex = int.Parse(args[2]);[m[41m[m
[32m+[m[32m                int population = int.Parse(args[3]);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                var generator = new Population(population, gen);[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                if (gen == 0)[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    generator.Generate();[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[32m                else[m[41m[m
[32m+[m[32m                {[m[41m[m
[32m+[m[32m                    generator.Evolve(maxIndex);[m[41m[m
[32m+[m[32m                }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                Console.WriteLine("OK");[m[41m[m
[32m+[m[41m[m
[32m+[m[32m                return;[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
 [m
         int numberOfCells = int.Parse(Console.ReadLine()); // 37[m
         for (int i = 0; i < numberOfCells; i++)[m
[36m@@ -249,7 +857,7 @@[m [mclass Player[m
             {[m
                 string possibleMove = Console.ReadLine();[m
                 game.possibleActions.Add(Action.Parse(possibleMove));[m
[31m-            }[m
[32m+[m[32m            }[m[41m            [m
 [m
             Action action = game.GetNextAction();[m
 [m
