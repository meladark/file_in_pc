using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace file_in_pc
{
    class find_file
    {
        public bool find(string path, string rex, ref int counter, ref int fi, ref List<string> well, Form1 form, ref List<string> not_over)
        {
            var files = Directory.GetFiles(path, rex);
            form.View_dir(path);          
            if (form.View_it_tree(files)) { not_over.Add(path); return true; }
            var direc = Directory.GetDirectories(path);
            counter += Directory.EnumerateFiles(path).Count();
            fi += files.Length;
            for (int i = 0; i < direc.Length; i++)
            {
                if (find(direc[i], rex, ref counter, ref fi, ref well, form, ref not_over))
                {
                    for (int j = i + 1; j < direc.Length; j++) { not_over.Add(direc[j]); }
                    return true;
                }
            }
            if (form.view_all_file(Directory.GetFiles(path).Length)) { not_over.Add(path); return true; } // не рационально, так как уже есть списко необходимых файлов по условию, но в ТЗ требуется количество всех файлов
            return false;
        }
    }
}
