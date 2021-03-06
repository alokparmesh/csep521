import matplotlib.pyplot as plt
import pandas as pd
import numpy as np
import warnings as warnings

def makeHeatMap(data, names, color, outputFileName):
    #to catch "falling back to Agg" warning
    with warnings.catch_warnings():
        warnings.simplefilter("ignore")
        #code source: http://stackoverflow.com/questions/14391959/heatmap-in-matplotlib-with-pcolor
        fig, ax = plt.subplots()
        #create the map w/ color bar legend
        heatmap = ax.pcolor(data, cmap=color)
        cbar = plt.colorbar(heatmap)

        # put the major ticks at the middle of each cell
        ax.set_xticks(np.arange(data.shape[0])+0.5, minor=False)
        ax.set_yticks(np.arange(data.shape[1])+0.5, minor=False)

        # want a more natural, table-like display
        ax.invert_yaxis()
        ax.xaxis.tick_top()

        ax.set_xticklabels(names)
        ax.set_yticklabels(names)

        plt.xticks(rotation=90)
        plt.tight_layout()

        plt.savefig(outputFileName, format = 'png')
        plt.close()

data = pd.read_csv("output.csv", index_col=False, header=None)
names = pd.read_csv("groups.csv", index_col=False, header=None)
makeHeatMap(data, names[0].values, plt.cm.Blues, "heatplot.png")

