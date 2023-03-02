/*
 
This is just code that I have written that I turned out to not need in case it becomes useful in the future. I just don't want it clogging up the main files

        public static string generateGenericLine(List<List<double>> data) { // string filePath) {
            string contents = "";
            var watch = new Stopwatch(); watch.Start();
            int counter = 0;
            
            foreach(var item in data) {
                string lines = $"v {item[0]} {item[1]} {item[2]}\nv {item[0]+0.001} {item[1]+0.001} {item[2]+0.001}\n";
                contents += lines;
                //Console.Write(lines);
                counter += 2;
            }
            
            for(int i=1; i < counter-2; i += 2) {
                string face = $"f {i} {i + 1} {i + 3} {i + 2}\n";
                contents += face;
            }
            
            Console.WriteLine(watch.ElapsedMilliseconds);
            return contents;
        }
 
 */