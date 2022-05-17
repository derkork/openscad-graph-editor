/**
 * Import test that verifies that documentation comments override inferred values.
 *
 * @param boolean [boolean] a boolean
 * @param number [number] a number
 * @param string [string] a string
 * @param vector2 [vector2] a vector2
 * @param vector3 [vector3] a vector3
 * @param array [array] an array
 * @param any [any] any type
 * @return [number] a number
 */
function doc_overrides(number,boolean,string,vector2,vector3,array,any=5) = 10;


function inferred(number=1, boolean=true, string="hello", vector2=[1,2], vector3=[1,(2),3], array=[1,2,3,4], any) = 10;