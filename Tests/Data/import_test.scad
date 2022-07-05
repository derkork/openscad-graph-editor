/**
 * Calculates the sum of the given numbers in an interesting way.
 * @param a the first number to add
 * @param b the second number to add
 * @returns [number] the sum of the given numbers
 */
function interesting_sum(a = 1, b = 2) = a + b + a + b + a + b - 2 * a - 2 * b;


/**
 * Renders an interesting cube.
 *
 * @param size the size of the cube
 * @param center [bool] whether to center the cube
 */
module interesting_cube(size=[1,2,3], center) {
    cube(size, center);
} 