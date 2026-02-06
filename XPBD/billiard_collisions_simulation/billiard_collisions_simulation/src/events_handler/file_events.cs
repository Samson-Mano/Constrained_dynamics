using billiard_collisions_simulation.src.global_variables;
using billiard_collisions_simulation.src.fe_objects;
using billiard_collisions_simulation.src.geom_objects;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace billiard_collisions_simulation.src.events_handler
{
    public static class file_events
    {
        private static double RoundToSixDigits(double value) => Math.Round(value, 6);


        //public static void import_binary_results(string filePath,
        //            ref feresults_data_store fe_results)
        //{
        //    // Reading the binary file

        //    using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open), Encoding.Default))
        //    {

        //        // ---- Read header ----
        //        double timeStart = reader.ReadDouble();
        //        double timeEnd = reader.ReadDouble();
        //        double dt = reader.ReadDouble();
        //        int N = (int)reader.ReadDouble();  // stored as double in Python

        //        if(N <= 0)
        //        {
        //            MessageBox.Show("Invalid number of data points in the binary file.", "Error loading bin file", 
        //                MessageBoxButtons.OK,
        //                MessageBoxIcon.Error);

        //            return;

        //        }

        //        // ---- Allocate arrays ----
        //        double[] time = new double[N];
        //        double[] displ = new double[N];
        //        double[] velo = new double[N];
        //        double[] accl = new double[N];

        //        // ---- Read all time-step records ----
        //        for (int i = 0; i < N; i++)
        //        {
        //            time[i] = reader.ReadDouble();
        //            displ[i] = reader.ReadDouble();
        //            velo[i] = reader.ReadDouble();
        //            accl[i] = reader.ReadDouble();
        //        }

        //        // Add to the list
        //        fe_results.store_results(
        //            RoundToSixDigits(timeStart),
        //            RoundToSixDigits(timeEnd),
        //            RoundToSixDigits(dt),
        //            N,
        //            time.ToList(),
        //            displ.ToList(),
        //            velo.ToList(),
        //            accl.ToList()
        //            );

        //        // OPTIONAL: Display confirmation
        //        MessageBox.Show(
        //            $"Loaded {N} data points.\nTime range: {timeStart} → {timeEnd}\nΔt = {dt}",
        //            "Binary File Loaded",
        //            MessageBoxButtons.OK,
        //            MessageBoxIcon.Information
        //        );

        //    }

        //}


        //public static void import_text_results(string filePath,
        //    ref feresults_data_store fe_results)
        //{
        //    // Reading the text file

        //    using (StreamReader reader = new StreamReader(filePath))
        //    {
        //        // ---- Read header ----
        //        double timeStart = double.Parse(reader.ReadLine().Split(':')[1]);
        //        double timeEnd = double.Parse(reader.ReadLine().Split(':')[1]);
        //        double dt = double.Parse(reader.ReadLine().Split(':')[1]);
        //        int N = int.Parse(reader.ReadLine().Split(':')[1]);

        //        if (N <= 0)
        //        {
        //            MessageBox.Show("Invalid number of data points in the text file.", "Error loading txt file",
        //                MessageBoxButtons.OK,
        //                MessageBoxIcon.Error);

        //            return;

        //        }

        //        // Skip the blank line
        //        reader.ReadLine();

        //        // Skip column header line: "time, displacement, velocity, acceleration"
        //        reader.ReadLine();

        //        // ---- Allocate arrays ----
        //        double[] time = new double[N];
        //        double[] displ = new double[N];
        //        double[] velo = new double[N];
        //        double[] accl = new double[N];

        //        // ---- Read data lines ----
        //        for (int i = 0; i < N; i++)
        //        {
        //            string line = reader.ReadLine();
        //            if (line == null)
        //                throw new Exception("Unexpected end of file while reading data rows.");

        //            string[] parts = line.Split(',');

        //            time[i] = double.Parse(parts[0]);
        //            displ[i] = double.Parse(parts[1]);
        //            velo[i] = double.Parse(parts[2]);
        //            accl[i] = double.Parse(parts[3]);
        //        }

        //        // Store results
        //        fe_results.store_results(
        //            RoundToSixDigits(timeStart),
        //            RoundToSixDigits(timeEnd),
        //            RoundToSixDigits(dt),
        //            N,
        //            time.ToList(),
        //            displ.ToList(),
        //            velo.ToList(),
        //            accl.ToList()
        //        );

        //        // Confirmation
        //        MessageBox.Show(
        //            $"Loaded {N} data points.\nTime range: {timeStart} → {timeEnd}\nΔt = {dt}",
        //            "Text File Loaded",
        //            MessageBoxButtons.OK,
        //            MessageBoxIcon.Information
        //        );

        //    }

        //}



    }

}
