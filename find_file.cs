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
        /// <summary>
        /// Функция непосредственного поиска необходимых файлов. Рекурсивная
        /// </summary>
        /// <param name="path"> Папка, в которой начинается поиск </param>
        /// <param name="rex"> Регулярное выражение, по которому будет производится поиск </param>
        /// <param name="form"> Ссылка на форму от которого оно запущено</param>
        /// <param name="not_over"> Список директорий, для возобнавления поиска</param>
        /// <returns></returns>
        public bool find(string path, string rex, Form1 form, ref List<string> not_over)
        {
            var files = Directory.GetFiles(path, rex);
            form.View_dir(path);          
            if (form.View_it_tree(files)) { not_over.Add(path); return true; }
            var direc = Directory.GetDirectories(path);
            for (int i = 0; i < direc.Length; i++)
            {
                if (find(direc[i], rex, form, ref not_over))
                {
                    for (int j = i + 1; j < direc.Length; j++) { not_over.Add(direc[j]); }
                    return true;
                }
            }
            if (form.view_all_file(Directory.EnumerateFiles(path).Count())) { not_over.Add(path); return true; } // не рационально, так как уже есть списко необходимых файлов по условию, но в ТЗ требуется количество всех файлов
            return false;
        }
    }
}
