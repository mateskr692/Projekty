#pragma once

//the tree contains a character code and its count. if it contains children, the count is sum of count of its children and ch is irrevalent
struct tree
{
	uint32_t count;
	uint8_t ch;

	struct tree *left, *right, *parent;
};

void tree_build_from_list(struct list **headp, struct tree **rootp);
void tree_delete(struct tree *root);
void tree_print(struct tree *root);