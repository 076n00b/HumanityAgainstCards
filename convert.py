#
#	convert.py
#	Convert text files into card packs
#

FILENAME_BLACK = 'black.txt'
FILENAME_WHITE = 'white.txt'

def ReadLines(filename):
	handle = open(filename, 'r')
	lines = handle.readlines()
	handle.close()
	
	return lines

if __name__ == '__main__':
	packOut = open('pack.json', 'w')
	packOut.write('{\n')
	packOut.write('\t\"WhiteCards\" :\n')
	packOut.write('\t[\n')
	
	whiteCards = ReadLines(FILENAME_WHITE)
	for line in whiteCards:
		safeLine = line
		safeLine = safeLine.replace('\"', '\\\"')
		safeLine = safeLine.replace('\n', '')
		
		if line == whiteCards[-1]:
			packOut.write('\t\t\"' + safeLine + '\"\n')
		else:
			packOut.write('\t\t\"' + safeLine + '\",\n')
	
	packOut.write('\t],\n')
	packOut.write('\t\"BlackCards\" :\n')
	packOut.write('\t[\n')
	
	blackCards = ReadLines(FILENAME_BLACK)
	for line in blackCards:
		safeLine = line
		safeLine = safeLine.replace('\"', '\\\"')
		safeLine = safeLine.replace('\n', '')
		
		if line == blackCards[-1]:
			packOut.write('\t\t\"' + safeLine + '\"\n')
		else:
			packOut.write('\t\t\"' + safeLine + '\",\n')
	
	packOut.write('\t]\n')
	packOut.write('}\n\n')
	packOut.close()
