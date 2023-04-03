# simple search and replace inside of a file
# usage: replace.py <file> <search> <replace>

import sys

if len(sys.argv) != 4:
    print("usage: replace.py <file> <search> <replace>")
    sys.exit(1)
    
file = sys.argv[1]
search = sys.argv[2]
replace = sys.argv[3]

f = open(file, "r")
data = f.read()
f.close()

data = data.replace(search, replace)

f = open(file, "w")
f.write(data)
f.close()

