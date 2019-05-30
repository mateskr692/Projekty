#include "stdafx.h"

void tree_build_from_list(struct list **headp, struct tree **rootp)
//connects the small tress into one huffman tree and leaves it as only element of list
{
	while (1)
	{
		struct list *low1 = list_remove_lowest(headp);
		struct list *low2 = list_remove_lowest(headp);

		if (low2 == NULL)
		{
			*rootp = low1->subtree;
			(*rootp)->parent = NULL;
			free(low1);
			return;
		}
		//connect trees, store them in low1
		struct tree *newtree = (struct tree*)malloc(sizeof(struct tree));
		newtree->left = low1->subtree;
		low1->subtree->parent = newtree;
		newtree->right = low2->subtree;
		low2->subtree->parent = newtree;
		newtree->count = (newtree->left->count) + (newtree->right->count);
		newtree->ch = 0;
		low1->subtree = newtree;
		//free low2, connect low1 back to list
		free(low2);
		low1->next = *headp;
		*headp = low1;
	}
}
void tree_delete(struct tree *root)
{
	if (root->left != NULL)
	{
		tree_delete(root->left);
		tree_delete(root->right);
		free(root);
	}
	else
		free(root);
}
void tree_print(struct tree *root)
{
	if (root->left == NULL)
	{
		printf("%d:%d ", root->ch, root->count);
		return;
	}
	else
	{
		printf("0 ");
		tree_print(root->left);
		tree_print(root->right);
		return;
	}
}