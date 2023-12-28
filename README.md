# Image Mosaic Generator
This project is a multi-threaded Image Mosaic Generator, written in C#. The application takes an input image and a set of tile images, then processes the images to create a mosaic output.

# Usage
The ImageMosaicGenerator program receives input through command-line arguments. The program accepts the following command-line arguments:

    -i or --image: Provides the path to the input image file.
    -t or --tiles: Specifies the path to the directory containing the tile images.
    -s or --tileSize: Defines the size of individual tiles.
    -w or --imageSize: Determines the size of the output image in terms of the number of tiles.
    -c or --threads: Configures the number of threads to be used in the generation of the mosaic.
    -h or --help: Displays usage information.

Example command:

dotnet ImageMosaicGenerator.dll -i path/to/image -t path/to/tiles -s tileSize -w imageSize -c threadCount

# Process
The program resizes and squares all tile images, then determines their average color. The input image is then resized based on the desired output size.

Following this, the application multi-threads the processing of the image, evaluating the color of each "pixel" (which corresponds to the size of the tiles) and locating the closest matching tile image in terms of color.

The selected tile is then drawn into the final mosaic image at the corresponding location.

After all pixels have been evaluated and corresponding tiles placed, the final image is saved as "yeet.png".

Please note: this is a command-line utility and expects specific syntax for command-line arguments. Improper entry may cause errors.
